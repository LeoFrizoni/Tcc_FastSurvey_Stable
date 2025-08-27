using FASTSURVEY.Dtos.Perguntas;
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.Validation
{
    public interface IValidationService
    {
        ServiceResult<bool> ValidatePesquisa(
            FASTSURVEY.Dtos.Pesquisas.CriarPesquisaRequest request
        );
        ServiceResult<bool> ValidatePergunta(CriarPerguntaRequest request);
        ServiceResult<bool> ValidatePergunta(AtualizarPerguntaRequest request);
        ServiceResult<bool> ValidateResposta(
            FASTSURVEY.Dtos.Respostas.CriarRespostaDiscursivaRequest request
        );
        ServiceResult<bool> ValidateFileUpload(
            IFormFile file,
            string[] allowedExtensions,
            long maxSizeInBytes
        );
        ServiceResult<bool> ValidateEmail(string email);
        ServiceResult<bool> ValidatePassword(string password);
        ServiceResult<bool> ValidateUsername(string username);
        List<string> GetValidationErrors();
    }

    public sealed class ValidationOptions
    {
        public int MaxPesquisaTitleLength { get; set; } = 120;
        public int MaxPesquisaDescriptionLength { get; set; } = 2000;
        public int MaxPerguntaTextLength { get; set; } = 500;
        public int MaxRespostaTextLength { get; set; } = 2000;
        public int MinPasswordLength { get; set; } = 8;
        public int MinUsernameLength { get; set; } = 3;
        public int MaxUsernameLength { get; set; } = 30;

        // Regras de opções
        public int MinOpcoesObjetiva { get; set; } = 2;
        public int MinOpcoesMultipla { get; set; } = 2;
        public int MaxOpcoes { get; set; } = 50;

        // Regras de arquivo
        public string[] AllowedFileExtensions { get; set; } =
            new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
        public int MaxFileSizeInMB { get; set; } = 10;
    }
}
