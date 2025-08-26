using System.Text.RegularExpressions;
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Dtos.Perguntas;
using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Dtos.Tipos;
using FASTSURVEY.Services.Result;
using Microsoft.Extensions.Options;

namespace FASTSURVEY.Services.Validation
{
    public class ValidationService : IValidationService
    {
        private readonly ValidationOptions _options;

        // Regex pré-compiladas
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );
        private static readonly Regex UsernameRegex = new(
            @"^[a-zA-Z0-9_-]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        // Apenas para GetValidationErrors()
        private List<string> _lastErrors = new();

        public ValidationService(IOptions<ValidationOptions> options) => _options = options.Value;

        // ============== PESQUISA ==============
        public ServiceResult<bool> ValidatePesquisa(
            FASTSURVEY.Dtos.Pesquisas.CriarPesquisaRequest request
        )
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Titulo))
                errors.Add("Título da pesquisa é obrigatório");
            else if (request.Titulo.Length > _options.MaxPesquisaTitleLength)
                errors.Add(
                    $"Título da pesquisa deve ter no máximo {_options.MaxPesquisaTitleLength} caracteres"
                );

            if (
                !string.IsNullOrEmpty(request.Descricao)
                && request.Descricao.Length > _options.MaxPesquisaDescriptionLength
            )
                errors.Add(
                    $"Descrição da pesquisa deve ter no máximo {_options.MaxPesquisaDescriptionLength} caracteres"
                );

            if (request.LoginId <= 0)
                errors.Add("ID do usuário é obrigatório");

            if (request.TipoPesquisaId <= 0)
                errors.Add("Tipo de pesquisa é obrigatório");

            if (!string.IsNullOrEmpty(request.TemplateJson))
            {
                try
                {
                    System.Text.Json.JsonDocument.Parse(request.TemplateJson);
                }
                catch
                {
                    errors.Add("Template JSON inválido");
                }
            }

            _lastErrors = errors;
            return errors.Count == 0
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail("VALIDATION_ERROR", errors);
        }

        // ============== PERGUNTA (CRIAR) ==============
        public ServiceResult<bool> ValidatePergunta(CriarPerguntaRequest request)
        {
            var errors = ValidatePerguntaBase(request);
            _lastErrors = errors;
            return errors.Count == 0
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail("VALIDATION_ERROR", errors);
        }

        // ============== PERGUNTA (ATUALIZAR) ==============
        public ServiceResult<bool> ValidatePergunta(AtualizarPerguntaRequest request)
        {
            var errors = ValidatePerguntaBase(request);

            if (request.PerguntaId <= 0)
                errors.Add("ID da pergunta é obrigatório");

            _lastErrors = errors;
            return errors.Count == 0
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail("VALIDATION_ERROR", errors);
        }

        // Reaproveitado pelos dois
        private List<string> ValidatePerguntaBase(PerguntaDtoBase request)
        {
            var errors = new List<string>();

            if (request.PesquisaId <= 0)
                errors.Add("ID da pesquisa é obrigatório");

            if (string.IsNullOrWhiteSpace(request.Texto))
                errors.Add("Texto da pergunta é obrigatório");
            else if (request.Texto.Length > _options.MaxPerguntaTextLength)
                errors.Add(
                    $"Texto da pergunta deve ter no máximo {_options.MaxPerguntaTextLength} caracteres"
                );

            if (request.Ordem < 0)
                errors.Add("Ordem da pergunta deve ser maior ou igual a zero");

            // Tipo (enum) — garantimos que seja um valor válido conhecido
            if (!Enum.IsDefined(typeof(TipoPerguntaDto), request.Tipo))
                errors.Add("Tipo de pergunta inválido");

            // Regras por tipo
            switch (request.Tipo)
            {
                case TipoPerguntaDto.Discursiva:
                    // Discursiva não deve ter opções (se vier, ignore ou valide)
                    if (request.Opcoes is { Count: > 0 })
                        errors.Add("Pergunta discursiva não deve definir opções");
                    // Gabarito discursivo? permitido se você quiser (ex.: exemplo de resposta),
                    // por padrão vamos permitir sem validações adicionais.
                    break;

                case TipoPerguntaDto.Objetiva:
                {
                    var count = request.Opcoes?.Count ?? 0;
                    if (count < _options.MinOpcoesObjetiva)
                        errors.Add(
                            $"Pergunta objetiva deve ter ao menos {_options.MinOpcoesObjetiva} opções"
                        );
                    if (count > _options.MaxOpcoes)
                        errors.Add($"Número máximo de opções excedido ({_options.MaxOpcoes})");

                    ValidateOpcoesTextos(request.Opcoes, errors);

                    if (request.TemGabarito)
                    {
                        if (request.OpcaoCorretaId is null)
                            errors.Add(
                                "Gabarito: é obrigatório informar 'OpcaoCorretaId' para pergunta objetiva"
                            );
                        if (request.OpcoesCorretasIds is { Count: > 0 })
                            errors.Add(
                                "Gabarito: pergunta objetiva aceita apenas uma opção correta"
                            );
                    }
                    break;
                }

                case TipoPerguntaDto.MultiplaEscolha:
                {
                    var count = request.Opcoes?.Count ?? 0;
                    if (count < _options.MinOpcoesMultipla)
                        errors.Add(
                            $"Pergunta de múltipla escolha deve ter ao menos {_options.MinOpcoesMultipla} opções"
                        );
                    if (count > _options.MaxOpcoes)
                        errors.Add($"Número máximo de opções excedido ({_options.MaxOpcoes})");

                    ValidateOpcoesTextos(request.Opcoes, errors);

                    // Por padrão, múltipla pode permitir múltiplas respostas
                    if (request.TemGabarito)
                    {
                        var qtd = request.OpcoesCorretasIds?.Count ?? 0;
                        if (qtd == 0)
                            errors.Add(
                                "Gabarito: informe 'OpcoesCorretasIds' para múltipla escolha"
                            );
                        if (request.OpcaoCorretaId is not null)
                            errors.Add("Gabarito: múltipla escolha não usa 'OpcaoCorretaId'");
                    }
                    break;
                }
            }

            // Campos auxiliares
            if (request.LimiteCaracteres is < 0)
                errors.Add("Limite de caracteres não pode ser negativo");
            if (request.TempoLimite is < 0)
                errors.Add("Tempo limite não pode ser negativo");

            // Anexo
            if (request.PermiteAnexo)
            {
                if (!string.IsNullOrWhiteSpace(request.TiposAnexoPermitidos))
                {
                    // opcional: validar formato "png,jpg,pdf"
                    var parts = request.TiposAnexoPermitidos.Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    );
                    if (parts.Length == 0)
                        errors.Add("Tipos de anexo permitidos estão em formato inválido");
                }
                if (request.TamanhoMaximoAnexo is <= 0)
                    errors.Add("Tamanho máximo do anexo deve ser maior que zero");
            }

            return errors;
        }

        private static void ValidateOpcoesTextos(
            List<OpcaoPerguntaRequest>? opcoes,
            List<string> errors
        )
        {
            if (opcoes is null)
                return;

            // Texto obrigatório e tamanho razoável
            foreach (var (op, idx) in opcoes.Select((o, i) => (o, i)))
            {
                if (op is null)
                {
                    errors.Add($"Opção #{idx + 1} inválida (nula)");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(op.Texto))
                    errors.Add($"Opção #{idx + 1}: texto é obrigatório");
                else if (op.Texto.Length > 300)
                    errors.Add($"Opção #{idx + 1}: texto deve ter no máximo 300 caracteres");
            }
        }

        // ============== RESPOSTA DISCURSIVA ==============
        public ServiceResult<bool> ValidateResposta(
            FASTSURVEY.Dtos.Respostas.CriarRespostaDiscursivaRequest request
        )
        {
            var errors = new List<string>();

            if (request.PerguntaId <= 0)
                errors.Add("ID da pergunta é obrigatório");

            if (string.IsNullOrWhiteSpace(request.Texto))
                errors.Add("Resposta discursiva deve conter texto");

            if (
                !string.IsNullOrEmpty(request.Texto)
                && request.Texto.Length > _options.MaxRespostaTextLength
            )
                errors.Add(
                    $"Texto da resposta deve ter no máximo {_options.MaxRespostaTextLength} caracteres"
                );

            _lastErrors = errors;
            return errors.Count == 0
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail("VALIDATION_ERROR", errors);
        }

        // ============== UPLOAD ==============
        public ServiceResult<bool> ValidateFileUpload(
            IFormFile file,
            string[] allowedExtensions,
            long maxSizeInBytes
        )
        {
            var errors = new List<string>();

            if (file is null)
                errors.Add("Arquivo é obrigatório");
            else
            {
                if (file.Length <= 0)
                    errors.Add("Arquivo vazio");
                if (file.Length > maxSizeInBytes)
                    errors.Add($"Arquivo deve ter no máximo {maxSizeInBytes / (1024 * 1024)}MB");

                var ext = (Path.GetExtension(file.FileName) ?? string.Empty).ToLowerInvariant();
                var allowed = (allowedExtensions ?? Array.Empty<string>())
                    .Select(e => e.ToLowerInvariant())
                    .ToHashSet();

                if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext))
                    errors.Add(
                        $"Tipo de arquivo não permitido. Extensões aceitas: {string.Join(", ", allowed)}"
                    );

                if (string.IsNullOrWhiteSpace(file.FileName))
                    errors.Add("Nome do arquivo é obrigatório");
                else if (file.FileName.Length > 255)
                    errors.Add("Nome do arquivo deve ter no máximo 255 caracteres");

                var invalidChars = Path.GetInvalidFileNameChars();
                if (file.FileName.Any(c => invalidChars.Contains(c)))
                    errors.Add("Nome do arquivo contém caracteres inválidos");
            }

            _lastErrors = errors;
            return errors.Count == 0
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail("VALIDATION_ERROR", errors);
        }

        // ============== EMAIL ==============
        public ServiceResult<bool> ValidateEmail(string email)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(email))
                errors.Add("Email é obrigatório");
            else
            {
                if (!EmailRegex.IsMatch(email))
                    errors.Add("Formato de email inválido");
                if (email.Length > 254)
                    errors.Add("Email deve ter no máximo 254 caracteres");
            }

            _lastErrors = errors;
            return errors.Count == 0
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail("VALIDATION_ERROR", errors);
        }

        // ============== SENHA ==============
        public ServiceResult<bool> ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
                errors.Add("Senha é obrigatória");
            else
            {
                if (password.Length < _options.MinPasswordLength)
                    errors.Add($"Senha deve ter no mínimo {_options.MinPasswordLength} caracteres");
                if (password.Length > 128)
                    errors.Add("Senha deve ter no máximo 128 caracteres");
                if (!password.Any(char.IsUpper))
                    errors.Add("Senha deve conter pelo menos uma letra maiúscula");
                if (!password.Any(char.IsLower))
                    errors.Add("Senha deve conter pelo menos uma letra minúscula");
                if (!password.Any(char.IsDigit))
                    errors.Add("Senha deve conter pelo menos um número");
                if (!password.Any(c => !char.IsLetterOrDigit(c)))
                    errors.Add("Senha deve conter pelo menos um caractere especial");
            }

            _lastErrors = errors;
            return errors.Count == 0
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail("VALIDATION_ERROR", errors);
        }

        // ============== USERNAME ==============
        public ServiceResult<bool> ValidateUsername(string username)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(username))
                errors.Add("Nome de usuário é obrigatório");
            else
            {
                if (username.Length < _options.MinUsernameLength)
                    errors.Add(
                        $"Nome de usuário deve ter no mínimo {_options.MinUsernameLength} caracteres"
                    );

                if (username.Length > _options.MaxUsernameLength)
                    errors.Add(
                        $"Nome de usuário deve ter no máximo {_options.MaxUsernameLength} caracteres"
                    );

                if (!UsernameRegex.IsMatch(username))
                    errors.Add(
                        "Nome de usuário deve conter apenas letras, números, hífen e underscore"
                    );

                var reservedWords = new[]
                {
                    "admin",
                    "administrator",
                    "root",
                    "system",
                    "guest",
                    "test",
                };
                if (reservedWords.Contains(username.ToLowerInvariant()))
                    errors.Add("Nome de usuário não pode ser uma palavra reservada");
            }

            _lastErrors = errors;
            return errors.Count == 0
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail("VALIDATION_ERROR", errors);
        }

        public List<string> GetValidationErrors() => new(_lastErrors);
    }
}
