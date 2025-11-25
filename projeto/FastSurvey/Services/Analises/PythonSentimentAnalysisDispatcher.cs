using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using FASTSURVEY.Services.Analises;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FASTSURVEY.Services.Analises
{
    public sealed class PythonSentimentAnalysisDispatcher : ISentimentAnalysisDispatcher
    {
        private readonly SentimentAiOptions _options;
        private readonly ILogger<PythonSentimentAnalysisDispatcher> _logger;
        private readonly IHostEnvironment _environment;
        private readonly string _workingDirectory;
        private readonly string _scriptFullPath;
        private readonly string? _sqlAlchemyConnection;

        public PythonSentimentAnalysisDispatcher(
            IOptions<SentimentAiOptions> options,
            ILogger<PythonSentimentAnalysisDispatcher> logger,
            IHostEnvironment environment,
            IConfiguration configuration
        )
        {
            _options = options.Value;
            _logger = logger;
            _environment = environment;
            _workingDirectory = ResolvePath(_options.WorkingDirectory);
            _scriptFullPath = ResolveScriptPath(_options.Script);
            _sqlAlchemyConnection = BuildSqlAlchemyConnection(
                configuration.GetConnectionString("DefaultConnection")
            );
        }

        public async Task AnalyzeAsync(int respostaId, string texto, CancellationToken ct = default)
        {
            if (!_options.Enabled)
            {
                _logger.LogDebug("Sentiment AI disabled. Skipping resposta {RespostaId}", respostaId);
                return;
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                _logger.LogTrace("Resposta {RespostaId} não possui texto para análise.", respostaId);
                return;
            }

            if (!File.Exists(_scriptFullPath))
            {
                _logger.LogWarning(
                    "Script de IA não encontrado em {Script}. Verifique appsettings ou execute npm install.",
                    _scriptFullPath
                );
                return;
            }

            var (processStart, tempFile) = BuildProcessStartInfo(respostaId, texto);
            using var process = new Process { StartInfo = processStart, EnableRaisingEvents = false };

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao iniciar processo de IA para resposta {RespostaId}", respostaId);
                CleanupTempFile(tempFile);
                return;
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            try
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                if (_options.TimeoutSeconds > 0)
                {
                    linkedCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));
                }

                await process.WaitForExitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                TryKillProcess(process);
                _logger.LogWarning(
                    "Tempo limite atingido ao analisar resposta {RespostaId}. Processo encerrado.",
                    respostaId
                );
                CleanupTempFile(tempFile);
                return;
            }

            var stdout = (await stdoutTask).Trim();
            var stderr = (await stderrTask).Trim();
            CleanupTempFile(tempFile);

            if (process.ExitCode != 0)
            {
                _logger.LogWarning(
                    "IA retornou código {ExitCode} para resposta {RespostaId}. Erro: {Error}",
                    process.ExitCode,
                    respostaId,
                    string.IsNullOrWhiteSpace(stderr) ? "(vazio)" : stderr
                );
                return;
            }

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                _logger.LogDebug("IA (stderr) resposta {RespostaId}: {Error}", respostaId, stderr);
            }

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                _logger.LogInformation("IA persistiu análise para resposta {RespostaId}: {Output}", respostaId, stdout);
            }
        }

        private (ProcessStartInfo Psi, string? TempFile) BuildProcessStartInfo(int respostaId, string texto)
        {
            var psi = new ProcessStartInfo
            {
                FileName = string.IsNullOrWhiteSpace(_options.PythonPath) ? "python" : _options.PythonPath,
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            if (!string.IsNullOrWhiteSpace(_sqlAlchemyConnection))
            {
                psi.EnvironmentVariables["SENTIMENT_DB_URL"] = _sqlAlchemyConnection;
            }

            var payloadMetadata = JsonSerializer.Serialize(new { ambiente = _environment.EnvironmentName });

            psi.ArgumentList.Add(_scriptFullPath);
            psi.ArgumentList.Add("--resposta-id");
            psi.ArgumentList.Add(respostaId.ToString(CultureInfo.InvariantCulture));

            var tempFile = PreparePayloadFileIfNeeded(texto);
            if (tempFile is not null)
            {
                psi.ArgumentList.Add("--input-file");
                psi.ArgumentList.Add(tempFile);
            }
            else
            {
                psi.ArgumentList.Add("--texto");
                psi.ArgumentList.Add(texto);
            }

            psi.ArgumentList.Add("--meta");
            psi.ArgumentList.Add(payloadMetadata);

            return (psi, tempFile);
        }

        private string? PreparePayloadFileIfNeeded(string texto)
        {
            if (texto.Length < 1800 && !texto.Contains('\n'))
            {
                return null;
            }

            var tempFile = Path.Combine(Path.GetTempPath(), $"fs_ai_{Guid.NewGuid():N}.txt");
            File.WriteAllText(tempFile, texto, Encoding.UTF8);
            return tempFile;
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return _environment.ContentRootPath;
            }

            return Path.IsPathRooted(path)
                ? path
                : Path.GetFullPath(Path.Combine(_environment.ContentRootPath, path));
        }

        private string ResolveScriptPath(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return Path.Combine(_workingDirectory, "scripts", "analyze_response.py");
            }

            return Path.IsPathRooted(script)
                ? script
                : Path.GetFullPath(Path.Combine(_workingDirectory, script));
        }

        private static string? BuildSqlAlchemyConnection(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return null;
            }

            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var username = Uri.EscapeDataString(builder.Username ?? string.Empty);
            var password = Uri.EscapeDataString(builder.Password ?? string.Empty);
            var host = builder.Host ?? "localhost";
            var port = builder.Port;
            var database = builder.Database ?? "postgres";
            return $"postgresql+psycopg2://{username}:{password}@{host}:{port}/{database}";
        }

        private static void TryKillProcess(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Ignorado
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // best effort
            }
        }

        private static void CleanupTempFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            TryDelete(path);
        }
    }
}
