using FASTSURVEY.Dtos.Anexos;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Text.RegularExpressions;
using AnexoEntity = SISTEMA_FASTSURVEY.MODEL.Models.Anexos;
using PerguntaEntity = SISTEMA_FASTSURVEY.MODEL.Models.Perguntas;

namespace FASTSURVEY.Services.Anexo
{
    public class AnexoService : IAnexoService
    {
        private readonly FastSurveyContext _ctx;

        public AnexoService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<ServiceResult<AnexoResponse>> UploadAsync(AnexoUploadRequest req, CancellationToken ct = default)
        {
            try
            {
                if (req.PesquisaId is null && req.PerguntaId is null)
                    return ServiceResult<AnexoResponse>.Fail("BadRequest", "Informe PesquisaId ou PerguntaId.");

                int? pesquisaIdFinal = req.PesquisaId;

                if (req.PesquisaId is not null)
                {
                    var existsPesquisa = await _ctx.Pesquisas.AnyAsync(p => p.Pesquisaid == req.PesquisaId.Value, ct);
                    if (!existsPesquisa)
                        return ServiceResult<AnexoResponse>.Fail("NotFound", "Pesquisa não encontrada.");
                }

                if (req.PerguntaId is not null)
                {
                    var pergunta = await _ctx.Perguntas
                        .AsNoTracking()
                        .Where(p => p.Perguntaid == req.PerguntaId.Value)
                        .Select(p => new { p.Perguntaid, p.Pesquisaid })
                        .FirstOrDefaultAsync(ct);

                    if (pergunta is null)
                        return ServiceResult<AnexoResponse>.Fail("NotFound", "Pergunta não encontrada.");

                    if (pesquisaIdFinal is null)
                        pesquisaIdFinal = pergunta.Pesquisaid;
                }

                if (pesquisaIdFinal is null)
                    return ServiceResult<AnexoResponse>.Fail("BadRequest", "Não foi possível determinar a PesquisaId para o anexo.");

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
                    Pesquisaid = pesquisaIdFinal.Value,
                    Perguntaid = req.PerguntaId,
                    Nome = internalName,
                    Nomeoriginal = originalName,
                    Extensao = safeExt,
                    Contenttype = req.Arquivo.ContentType,
                    Tamanhobytes = bytes.LongLength,
                    Base64data = base64
                };

                _ctx.Anexos.Add(entity);
                await _ctx.SaveChangesAsync(ct);

                return ServiceResult<AnexoResponse>.Ok(ToResponse(entity));
            }
            catch (Exception ex)
            {
                return ServiceResult<AnexoResponse>.Fail("Error", "Falha ao salvar anexo: " + ex.Message);
            }
        }

        public async Task<ServiceResult<AnexoResponse>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctx.Anexos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Anexoid == id, ct);

            if (entity is null)
                return ServiceResult<AnexoResponse>.Fail("NotFound", "Anexo não encontrado.");

            return ServiceResult<AnexoResponse>.Ok(ToResponse(entity));
        }

        public async Task<ServiceResult<List<AnexoResponse>>> ListByPesquisaAsync(int pesquisaId, CancellationToken ct = default)
        {
            var list = await _ctx.Anexos.AsNoTracking()
                .Where(a => a.Pesquisaid == pesquisaId)
                .Select(a => ToResponse(a))
                .ToListAsync(ct);

            return ServiceResult<List<AnexoResponse>>.Ok(list);
        }

        public async Task<ServiceResult<List<AnexoResponse>>> ListByPerguntaAsync(int perguntaId, CancellationToken ct = default)
        {
            var list = await _ctx.Anexos.AsNoTracking()
                .Where(a => a.Perguntaid == perguntaId)
                .Select(a => ToResponse(a))
                .ToListAsync(ct);

            return ServiceResult<List<AnexoResponse>>.Ok(list);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctx.Anexos.FirstOrDefaultAsync(a => a.Anexoid == id, ct);
            if (entity is null)
                return ServiceResult<bool>.Fail("NotFound", "Anexo não encontrado.");

            // Se quiser bloquear remoção quando houver resposta que referencie o anexo, valide aqui.

            _ctx.Anexos.Remove(entity);
            await _ctx.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<(byte[] Bytes, string ContentType, string FileName)>> DownloadAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctx.Anexos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Anexoid == id, ct);

            if (entity is null)
                return ServiceResult<(byte[], string, string)>.Fail("NotFound", "Anexo não encontrado.");

            if (string.IsNullOrWhiteSpace(entity.Base64data))
                return ServiceResult<(byte[], string, string)>.Fail("BadRequest", "Anexo sem dados armazenados.");

            byte[] bytes;
            try { bytes = Convert.FromBase64String(entity.Base64data); }
            catch { return ServiceResult<(byte[], string, string)>.Fail("Error", "Dados do anexo corrompidos."); }

            var fileName = string.IsNullOrWhiteSpace(entity.Nomeoriginal) ? (entity.Nome ?? $"anexo_{id}") : entity.Nomeoriginal;
            var contentType = string.IsNullOrWhiteSpace(entity.Contenttype) ? "application/octet-stream" : entity.Contenttype;

            return ServiceResult<(byte[], string, string)>.Ok((bytes, contentType, fileName));
        }

        private static AnexoResponse ToResponse(AnexoEntity a)
        {
            // Se quiser DataCriacao real, crie a coluna no DB.
            return new AnexoResponse
            {
                Id = a.Anexoid,
                PesquisaId = a.Pesquisaid,
                PerguntaId = a.Perguntaid,
                NomeArquivo = a.Nomeoriginal ?? a.Nome ?? $"anexo_{a.Anexoid}",
                Url = $"/api/anexos/{a.Anexoid}/download",
                ContentType = a.Contenttype,
                TamanhoBytes = a.Tamanhobytes,
                DataCriacao = DateTime.UtcNow
            };
        }
    }
}
