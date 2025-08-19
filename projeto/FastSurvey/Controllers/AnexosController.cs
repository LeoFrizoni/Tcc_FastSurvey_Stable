using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using FASTSURVEY.Models;                    // AnexoUploadModel
using SISTEMA_FASTSURVEY.MODEL.Models;      // FastSurveyContext, entidades
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AnexosController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryAnexos _repoAnexos;

        // 10 MB
        private const long MaxUploadBytes = 10L * 1024 * 1024;
        private static readonly string[] AllowedContentTypes = { "image/png", "image/jpeg", "application/pdf" };

        public AnexosController(FastSurveyContext context)
        {
            _context = context;
            _repoAnexos = new RepositoryAnexos(_context, true);
        }

        // =========================
        // Helpers
        // =========================

        private static bool MagicBytesValid(Stream stream, string contentType)
        {
            if (!stream.CanSeek) return false;
            var pos = stream.Position;
            try
            {
                Span<byte> header = stackalloc byte[8];
                var read = stream.Read(header);
                stream.Position = pos;

                ReadOnlySpan<byte> h = header.Slice(0, read);

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (contentType == "image/png")
                {
                    ReadOnlySpan<byte> png = stackalloc byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                    return h.Length >= 8 && h.Slice(0, 8).SequenceEqual(png);
                }

                // JPEG: FF D8
                if (contentType == "image/jpeg")
                {
                    return h.Length >= 2 && h[0] == 0xFF && h[1] == 0xD8;
                }

                // PDF: %PDF
                if (contentType == "application/pdf")
                {
                    return h.Length >= 4 && h[0] == 0x25 && h[1] == 0x50 && h[2] == 0x44 && h[3] == 0x46;
                }

                return false;
            }
            catch
            {
                if (stream.CanSeek) stream.Position = pos;
                return false;
            }
        }

        private static string NormalizeContentType(string? ct)
        {
            ct = (ct ?? "").Trim().ToLowerInvariant();
            return ct switch
            {
                "image/jpg" => "image/jpeg",
                "image/jpeg" => "image/jpeg",
                "image/png" => "image/png",
                "application/pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }

        private static string ExtensionFor(string contentType, string? originalExt)
        {
            return contentType switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "application/pdf" => ".pdf",
                _ => string.IsNullOrWhiteSpace(originalExt) ? "" : originalExt
            };
        }

        private string BuildDownloadUrl(int anexoId)
            => Url.Action(nameof(Download), "Anexos", new { id = anexoId }, Request.Scheme) ?? string.Empty;

        // =========================
        // Endpoints
        // =========================

        /// <summary>Upload de anexo (Base64 no banco). multipart/form-data: Arquivo, PerguntaId (opcional).</summary>
        [HttpPost("{pesquisaId:int}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxUploadBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
        public async Task<IActionResult> CadastrarAnexo(
            int pesquisaId,
            [FromForm] AnexoUploadModel form,
            CancellationToken ct)
        {
            try
            {
                if (form?.Arquivo == null || form.Arquivo.Length == 0)
                    return BadRequest("Anexo não pode ser nulo ou vazio.");

                if (form.Arquivo.Length > MaxUploadBytes)
                    return StatusCode(StatusCodes.Status413PayloadTooLarge, $"Arquivo excede {MaxUploadBytes / (1024 * 1024)} MB.");

                // Confere existência da pesquisa
                var existePesquisa = await _context.pesquisas.AsNoTracking()
                    .AnyAsync(p => p.pesquisaid == pesquisaId, ct);
                if (!existePesquisa)
                    return NotFound("Pesquisa não encontrada.");

                var incomingCt = NormalizeContentType(form.Arquivo.ContentType);
                if (!AllowedContentTypes.Contains(incomingCt))
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType, "Tipo de arquivo não permitido. Use PNG, JPEG ou PDF.");

                // Validação dos magic bytes
                await using (var peek = form.Arquivo.OpenReadStream())
                {
                    if (!MagicBytesValid(peek, incomingCt))
                        return BadRequest("Conteúdo do arquivo não corresponde ao tipo informado.");
                }

                // Lê conteúdo e converte para base64
                string base64Data;
                await using (var ms = new MemoryStream())
                {
                    await form.Arquivo.CopyToAsync(ms, ct);
                    base64Data = Convert.ToBase64String(ms.ToArray());
                }

                var originalExt = Path.GetExtension(form.Arquivo.FileName);
                var ext = ExtensionFor(incomingCt, originalExt);
                var nomeServidor = $"{Guid.NewGuid():N}";

                var novo = new anexos
                {
                    nome = nomeServidor,
                    extensao = ext,
                    pesquisaid = pesquisaId,
                    perguntaid = form.PerguntaId,
                    nomeoriginal = form.Arquivo.FileName,
                    contenttype = incomingCt,
                    tamanhobytes = form.Arquivo.Length,
                    base64data = base64Data
                };

                await _repoAnexos.IncluirAsync(novo);

                var dto = new
                {
                    mensagem = "Anexo cadastrado com sucesso.",
                    anexoid = novo.anexoid,
                    pesquisaId,
                    perguntaId = form.PerguntaId,
                    nomeOriginal = novo.nomeoriginal,
                    tamanhoBytes = novo.tamanhobytes,
                    contentType = novo.contenttype,
                    extensao = novo.extensao,
                    downloadUrl = BuildDownloadUrl(novo.anexoid)
                };

                return CreatedAtAction(nameof(Download), new { id = novo.anexoid }, dto);
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao cadastrar anexo: {ex.Message}");
            }
        }

        /// <summary>Metadados de um anexo (sem Base64).</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obter(int id, CancellationToken ct)
        {
            var a = await _context.anexos.AsNoTracking()
                .Where(x => x.anexoid == id)
                .Select(x => new
                {
                    x.anexoid,
                    x.pesquisaid,
                    x.perguntaid,
                    nomeServidor = x.nome + x.extensao,
                    nomeOriginal = x.nomeoriginal,
                    tamanhoBytes = x.tamanhobytes,
                    contentType = x.contenttype,
                    extensao = x.extensao,
                    downloadUrl = BuildDownloadUrl(x.anexoid)
                })
                .FirstOrDefaultAsync(ct);

            return a == null ? NotFound("Anexo não encontrado.") : Ok(a);
        }

        /// <summary>Lista anexos de uma pesquisa (sem Base64).</summary>
        [HttpGet("pesquisa/{pesquisaId:int}")]
        public async Task<IActionResult> ListarPorPesquisa(int pesquisaId, CancellationToken ct)
        {
            try
            {
                var lista = await _context.anexos.AsNoTracking()
                    .Where(a => a.pesquisaid == pesquisaId)
                    .OrderBy(a => a.anexoid)
                    .Select(a => new
                    {
                        a.anexoid,
                        a.pesquisaid,
                        a.perguntaid,
                        nomeServidor = a.nome + a.extensao,
                        nomeOriginal = a.nomeoriginal,
                        tamanhoBytes = a.tamanhobytes,
                        contentType = a.contenttype,
                        extensao = a.extensao,
                        downloadUrl = BuildDownloadUrl(a.anexoid)
                    })
                    .ToListAsync(ct);

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao listar anexos: {ex.Message}");
            }
        }

        /// <summary>Lista anexos de uma pergunta (sem Base64).</summary>
        [HttpGet("pergunta/{perguntaId:int}")]
        public async Task<IActionResult> ListarPorPergunta(int perguntaId, CancellationToken ct)
        {
            try
            {
                var lista = await _context.anexos.AsNoTracking()
                    .Where(a => a.perguntaid == perguntaId)
                    .OrderBy(a => a.anexoid)
                    .Select(a => new
                    {
                        a.anexoid,
                        a.pesquisaid,
                        a.perguntaid,
                        nomeServidor = a.nome + a.extensao,
                        nomeOriginal = a.nomeoriginal,
                        tamanhoBytes = a.tamanhobytes,
                        contentType = a.contenttype,
                        extensao = a.extensao,
                        downloadUrl = BuildDownloadUrl(a.anexoid)
                    })
                    .ToListAsync(ct);

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao listar anexos: {ex.Message}");
            }
        }

        /// <summary>Download do anexo (reconstrói a partir do Base64).</summary>
        [HttpGet("download/{id:int}")]
        public async Task<IActionResult> Download(int id, CancellationToken ct)
        {
            var anexo = await _context.anexos.AsNoTracking()
                .FirstOrDefaultAsync(a => a.anexoid == id, ct);

            if (anexo == null) return NotFound("Anexo não encontrado.");
            if (string.IsNullOrWhiteSpace(anexo.base64data))
                return NotFound("Conteúdo Base64 não encontrado para este anexo.");

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(anexo.base64data);
            }
            catch
            {
                return Problem("Falha ao decodificar o conteúdo Base64.");
            }

            var mime = string.IsNullOrWhiteSpace(anexo.contenttype)
                ? "application/octet-stream"
                : anexo.contenttype;

            var nomeDownload = !string.IsNullOrWhiteSpace(anexo.nomeoriginal)
                ? anexo.nomeoriginal
                : (anexo.nome + anexo.extensao);

            // attachment por padrão
            return File(bytes, mime, fileDownloadName: nomeDownload);
        }

        /// <summary>Exclui um anexo (somente banco; não há arquivo físico).</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                if (id <= 0) return BadRequest("ID inválido.");

                var anexo = await _repoAnexos.SelecionarChaveAsync(id, ct);
                if (anexo == null) return NotFound("Anexo não encontrado.");

                await _repoAnexos.ExcluirAsync(anexo, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao excluir anexo: {ex.Message}");
            }
        }

        /// <summary>Metadados + Base64 (payload grande; use com cautela).</summary>
        [HttpGet("{id:int}/inline")]
        public async Task<IActionResult> ObterInline(int id, CancellationToken ct)
        {
            var a = await _context.anexos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.anexoid == id, ct);

            if (a == null) return NotFound("Anexo não encontrado.");

            return Ok(new
            {
                a.anexoid,
                a.pesquisaid,
                a.perguntaid,
                a.nomeoriginal,
                a.contenttype,
                a.extensao,
                a.tamanhobytes,
                base64Data = a.base64data
            });
        }
    }
}
