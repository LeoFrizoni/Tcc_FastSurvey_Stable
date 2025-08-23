// FASTSURVEY/Services/Anexo/IAnexoService.cs
using FASTSURVEY.Dtos.Anexos;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.Anexo
{
    public interface IAnexoService
    {
        Task<ServiceResult<AnexoResponse>> UploadAsync(AnexoUploadRequest req, CancellationToken ct = default);

        Task<ServiceResult<AnexoResponse>> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ServiceResult<List<AnexoResponse>>> ListByPesquisaAsync(int pesquisaId, CancellationToken ct = default);

        Task<ServiceResult<List<AnexoResponse>>> ListByPerguntaAsync(int perguntaId, CancellationToken ct = default);

        Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);

        /// Retorna bytes + contentType + nome para download
        Task<ServiceResult<(byte[] Bytes, string ContentType, string FileName)>> DownloadAsync(int id, CancellationToken ct = default);
    }
}
