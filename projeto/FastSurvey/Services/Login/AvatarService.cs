#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Login.Avatar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Login
{
    public sealed class AvatarService : IAvatarService
    {
        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp",
        };
        private const long MaxSizeBytes = 5L * 1024 * 1024; // 5MB

        private readonly FastSurveyContext _ctx;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AvatarService>? _logger;

        public AvatarService(
            FastSurveyContext ctx,
            IWebHostEnvironment env,
            ILogger<AvatarService>? logger = null
        )
        {
            _ctx = ctx;
            _env = env;
            _logger = logger;
        }

        public async Task<AvatarResponse> UploadAsync(
            int loginId,
            AvatarUploadRequest req,
            CancellationToken ct = default
        )
        {
            if (req is null || req.File is null)
                throw new ArgumentException("Arquivo é obrigatório.");

            // --- Validação básica (pode trocar para ValidationService se preferir) ---
            var ext = (Path.GetExtension(req.File.FileName) ?? string.Empty).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new ArgumentException(
                    $"Extensão não suportada. Permitidas: {string.Join(", ", AllowedExtensions)}."
                );

            if (req.File.Length <= 0)
                throw new ArgumentException("Arquivo vazio.");

            if (req.File.Length > MaxSizeBytes)
                throw new ArgumentException($"Arquivo excede {MaxSizeBytes / (1024 * 1024)}MB.");

            var contentType = req.File.ContentType?.ToLowerInvariant() ?? "";
            if (!(contentType.StartsWith("image/")))
                throw new ArgumentException("Content-Type inválido para imagem.");

            // Valida usuário
            var login =
                await _ctx.Login.FirstOrDefaultAsync(l => l.LoginId == loginId, ct)
                ?? throw new InvalidOperationException("Usuário inexistente.");

            // Prepara diretório
            var webroot = _env.WebRootPath ?? "wwwroot";
            var targetDir = Path.Combine(webroot, "uploads", "avatars");
            Directory.CreateDirectory(targetDir);

            // Nome seguro e único
            var fileName = $"{loginId}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(targetDir, fileName);
            var relUrl = $"/uploads/avatars/{fileName}";

            // Se houver avatar anterior, guarda para remoção depois de persistir novo
            var avatar = await _ctx.LoginAvatar.FirstOrDefaultAsync(a => a.LoginId == loginId, ct);
            var oldRelUrl = avatar?.StorageUrl;

            // Grava arquivo em disco
            try
            {
                await using var fs = new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true
                );
                await req.File.CopyToAsync(fs, ct);
            }
            catch
            {
                // se falhar gravação, não mexe em DB
                throw;
            }

            // Atualiza DB
            try
            {
                if (avatar is null)
                {
                    avatar = new LoginAvatar
                    {
                        LoginId = loginId,
                        StorageUrl = relUrl,
                        NomeOriginal = req.File.FileName,
                        ContentType = req.File.ContentType,
                        TamanhoBytes = req.File.Length,
                        CriadoEm = DateTime.UtcNow,
                        Versao = 1,
                    };
                    _ctx.LoginAvatar.Add(avatar);
                }
                else
                {
                    avatar.StorageUrl = relUrl;
                    avatar.NomeOriginal = req.File.FileName;
                    avatar.ContentType = req.File.ContentType;
                    avatar.TamanhoBytes = req.File.Length;
                    avatar.AtualizadoEm = DateTime.UtcNow;
                    avatar.Versao = (avatar.Versao <= 0 ? 1 : avatar.Versao + 1);
                }

                await _ctx.SaveChangesAsync(ct);
            }
            catch
            {
                // rollback do arquivo novo se DB falhar
                TryDeletePhysical(fullPath);
                throw;
            }

            // Após persistir, tenta apagar o arquivo anterior (melhor esforço)
            if (!string.IsNullOrWhiteSpace(oldRelUrl))
            {
                var oldPath = MapRelativeToPhysical(webroot, oldRelUrl);
                TryDeletePhysical(oldPath);
            }

            return new AvatarResponse { Url = avatar.StorageUrl, Versao = avatar.Versao };
        }

        public async Task<bool> RemoverAsync(int loginId, CancellationToken ct = default)
        {
            var avatar = await _ctx.LoginAvatar.FirstOrDefaultAsync(a => a.LoginId == loginId, ct);
            if (avatar is null)
                return true;

            var webroot = _env.WebRootPath ?? "wwwroot";
            var physical = MapRelativeToPhysical(webroot, avatar.StorageUrl);

            _ctx.LoginAvatar.Remove(avatar);
            await _ctx.SaveChangesAsync(ct);

            // melhor esforço para remover arquivo do disco
            TryDeletePhysical(physical);

            return true;
        }

        // ---------- helpers ----------

        private static string MapRelativeToPhysical(string webroot, string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            // normaliza "/uploads/avatars/file.jpg" -> "uploads/avatars/file.jpg"
            var rel = url.Replace('\\', '/');
            if (rel.StartsWith("/"))
                rel = rel[1..];

            return Path.Combine(webroot, rel);
        }

        private void TryDeletePhysical(string? path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Falha ao excluir arquivo de avatar: {Path}", path);
            }
        }
    }
}
