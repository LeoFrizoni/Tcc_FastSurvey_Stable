#nullable enable
using FASTSURVEY.Dtos.Anexos;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
// Alias para evitar conflito de nomes
using AnexoEntity = SISTEMA_FASTSURVEY.MODEL.Models.Anexos;

namespace FASTSURVEY.Services.Anexo
{
    public class AnexoService : IAnexoService
    {
        private readonly FastSurveyContext _ctx;

        public AnexoService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<ServiceResult<AnexoResponse>> UploadAsync(
            AnexoUploadRequest req,
            CancellationToken ct = default
        )
        {
            try
            {
                if (req.PesquisaId is null && req.PerguntaId is null)
                    return ServiceResult<AnexoResponse>.Fail(
                        "BadRequest",
                        "Informe PesquisaId ou PerguntaId."
                    );

                int? pesquisaIdFinal = req.PesquisaId;

                if (req.PesquisaId is not null)
                {
                    var existsPesquisa = await _ctx.Pesquisas.AnyAsync(
                        p => p.PesquisaId == req.PesquisaId.Value,
                        ct
                    );
                    if (!existsPesquisa)
                        return ServiceResult<AnexoResponse>.Fail(
                            "NotFound",
                            "Pesquisa não encontrada."
                        );
                }

                if (req.PerguntaId is not null)
                {
                    var pergunta = await _ctx
                        .Perguntas.AsNoTracking()
                        .Where(p => p.PerguntaId == req.PerguntaId.Value)
                        .Select(p => new { p.PerguntaId, p.PesquisaId })
                        .FirstOrDefaultAsync(ct);

                    if (pergunta is null)
                        return ServiceResult<AnexoResponse>.Fail(
                            "NotFound",
                            "Pergunta não encontrada."
                        );

                    if (req.PesquisaId is not null && req.PesquisaId.Value != pergunta.PesquisaId)
                        return ServiceResult<AnexoResponse>.Fail(
                            "BadRequest",
                            "Pergunta não pertence à Pesquisa informada."
                        );

                    if (pesquisaIdFinal is null)
                        pesquisaIdFinal = pergunta.PesquisaId;
                }

                if (pesquisaIdFinal is null)
                    return ServiceResult<AnexoResponse>.Fail(
                        "BadRequest",
                        "Não foi possível determinar a PesquisaId para o anexo."
                    );

                // Lê bytes do arquivo
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await req.Arquivo.CopyToAsync(ms, ct);
                    bytes = ms.ToArray();
                }

                var base64 = Convert.ToBase64String(bytes);
                var originalName = req.Arquivo.FileName ?? "arquivo";
                var ext = Path.GetExtension(originalName)?.TrimStart('.').ToLowerInvariant() ?? "";
                var safeExt = string.IsNullOrWhiteSpace(ext) ? "" : ext;

                // Nome interno simples e único
                var internalName = $"{Guid.NewGuid():N}{(safeExt != "" ? "." + safeExt : "")}";

                var entity = new AnexoEntity
                {
                    PesquisaId = pesquisaIdFinal.Value,
                    PerguntaId = req.PerguntaId,
                    Nome = internalName,
                    NomeOriginal = originalName,
                    Extensao = safeExt,
                    ContentType = req.Arquivo.ContentType,
                    TamanhoBytes = bytes.LongLength,
                    Base64Data = base64,
                };

                _ctx.Anexos.Add(entity);
                await _ctx.SaveChangesAsync(ct);

                return ServiceResult<AnexoResponse>.Ok(ToResponse(entity));
            }
            catch (Exception ex)
            {
                return ServiceResult<AnexoResponse>.Fail(
                    "Error",
                    "Falha ao salvar anexo: " + ex.Message
                );
            }
        }

        public async Task<ServiceResult<AnexoResponse>> GetByIdAsync(
            int id,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx
                .Anexos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnexoId == id, ct);

            if (entity is null)
                return ServiceResult<AnexoResponse>.Fail("NotFound", "Anexo não encontrado.");

            return ServiceResult<AnexoResponse>.Ok(ToResponse(entity));
        }

        public async Task<ServiceResult<List<AnexoResponse>>> ListByPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            var list = await _ctx
                .Anexos.AsNoTracking()
                .Where(a => a.PesquisaId == pesquisaId)
                .Select(a => ToResponse(a))
                .ToListAsync(ct);

            return ServiceResult<List<AnexoResponse>>.Ok(list);
        }

        public async Task<ServiceResult<List<AnexoResponse>>> ListByPerguntaAsync(
            int perguntaId,
            CancellationToken ct = default
        )
        {
            var list = await _ctx
                .Anexos.AsNoTracking()
                .Where(a => a.PerguntaId == perguntaId)
                .Select(a => ToResponse(a))
                .ToListAsync(ct);

            return ServiceResult<List<AnexoResponse>>.Ok(list);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctx.Anexos.FirstOrDefaultAsync(a => a.AnexoId == id, ct);
            if (entity is null)
                return ServiceResult<bool>.Fail("NotFound", "Anexo não encontrado.");

            _ctx.Anexos.Remove(entity);
            await _ctx.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<
            ServiceResult<(byte[] Bytes, string ContentType, string FileName)>
        > DownloadAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctx
                .Anexos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnexoId == id, ct);

            if (entity is null)
                return ServiceResult<(byte[] Bytes, string ContentType, string FileName)>.Fail(
                    "NotFound",
                    "Anexo não encontrado."
                );

            if (string.IsNullOrWhiteSpace(entity.Base64Data))
                return ServiceResult<(byte[] Bytes, string ContentType, string FileName)>.Fail(
                    "Error",
                    "Dados do anexo ausentes."
                );

            try
            {
                var bytes = Convert.FromBase64String(entity.Base64Data);
                var fileName = !string.IsNullOrWhiteSpace(entity.NomeOriginal)
                    ? entity.NomeOriginal
                    : entity.Nome;
                var contentType = string.IsNullOrWhiteSpace(entity.ContentType)
                    ? "application/octet-stream"
                    : entity.ContentType;

                return ServiceResult<(byte[] Bytes, string ContentType, string FileName)>.Ok(
                    (bytes, contentType, fileName)
                );
            }
            catch (FormatException)
            {
                return ServiceResult<(byte[] Bytes, string ContentType, string FileName)>.Fail(
                    "Error",
                    "Base64 inválida para o anexo."
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<(byte[] Bytes, string ContentType, string FileName)>.Fail(
                    "Error",
                    "Falha ao converter dados do anexo: " + ex.Message
                );
            }
        }

        private static AnexoResponse ToResponse(AnexoEntity a) =>
            new()
            {
                AnexoId = a.AnexoId,
                PesquisaId = a.PesquisaId,
                PerguntaId = a.PerguntaId,
                Nome = a.Nome,
                NomeOriginal = a.NomeOriginal,
                Extensao = a.Extensao,
                ContentType = a.ContentType,
                TamanhoBytes = a.TamanhoBytes,
            };
    }
}
