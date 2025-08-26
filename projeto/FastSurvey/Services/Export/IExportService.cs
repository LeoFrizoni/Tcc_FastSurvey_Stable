#nullable enable
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.Export
{
    public interface IExportService
    {
        Task<ServiceResult<ExportFileResult>> ExportarPesquisaPDFAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
        Task<ServiceResult<ExportFileResult>> ExportarResultadosPDFAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
        Task<ServiceResult<ExportFileResult>> ExportarResultadosExcelAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
        Task<ServiceResult<ExportFileResult>> ExportarRespostasCSVAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
        Task<ServiceResult<List<ExportFileResult>>> ExportarTodosFormatosAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
    }

    public sealed class ExportFileResult
    {
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
