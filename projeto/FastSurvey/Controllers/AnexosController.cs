using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FASTSURVEY.Models;                    // AnexoUploadModel
using SISTEMA_FASTSURVEY.MODEL.Models;      // entidades (anexos, etc.)
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnexosController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryAnexos _repositoryAnexos;

        // ❌ Não precisamos mais de caminho físico/disco
        public AnexosController(FastSurveyContext context)
        {
            _context = context;
            _repositoryAnexos = new RepositoryAnexos(_context, true);
        }

        /// <summary>
        /// Upload de anexo para uma pesquisa. Envie 'anexo' como campo de arquivo (multipart/form-data).
        /// Agora o conteúdo é salvo em Base64 no banco (não escreve mais em disco).
        /// </summary>
        [HttpPost("{pesquisaId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CadastrarAnexo(int pesquisaId, [FromForm] AnexoUploadModel form)
        {
            try
            {
                if (form?.Arquivo == null || form.Arquivo.Length == 0)
                    return BadRequest("Anexo não pode ser nulo ou vazio.");

                // ✅ Regras de segurança/opcionais
                var maxBytes = 10 * 1024 * 1024; // 10 MB
                if (form.Arquivo.Length > maxBytes)
                    return StatusCode(413, "Arquivo excede o limite permitido (10 MB).");

                // Tipos permitidos (ajuste conforme necessidade)
                var contentType = form.Arquivo.ContentType?.ToLower() ?? "application/octet-stream";
                var tiposPermitidos = new[]
                {
                    "image/png", "image/jpeg", "image/jpg", "application/pdf"
                };
                if (!tiposPermitidos.Contains(contentType))
                    return BadRequest("Tipo de arquivo não permitido.");

                // Lê o arquivo em memória e converte para Base64 (sem prefixo data:)
                string base64Data;
                await using (var ms = new MemoryStream())
                {
                    await form.Arquivo.CopyToAsync(ms);
                    var bytes = ms.ToArray();
                    base64Data = Convert.ToBase64String(bytes);
                }

                var ext = Path.GetExtension(form.Arquivo.FileName); // ex.: ".png"
                var nomeServidor = $"{Guid.NewGuid():N}";            // nome lógico (sem uso em disco)

                // Persiste metadados + conteúdo Base64
                var novoAnexo = new anexos
                {
                    nome = nomeServidor,                   // mantém o padrão de "nome" lógico
                    extensao = ext,
                    pesquisaid = pesquisaId,
                    perguntaid = form.PerguntaId,         // opcional
                    nomeoriginal = form.Arquivo.FileName,
                    contenttype = contentType,
                    tamanhobytes = form.Arquivo.Length,
                    base64data = base64Data               // ✅ CONTEÚDO EM BASE64 NO BANCO
                };

                await _repositoryAnexos.IncluirAsync(novoAnexo);

                // Mantém um endpoint de download que agora reconstrói a partir do Base64
                var downloadUrl = Url.Action(nameof(Download), "Anexos", new { id = novoAnexo.anexoid }, Request.Scheme);

                return Ok(new
                {
                    mensagem = "Anexo cadastrado com sucesso (armazenado em Base64).",
                    anexoid = novoAnexo.anexoid,
                    pesquisaId,
                    perguntaId = form.PerguntaId,
                    nomeOriginal = novoAnexo.nomeoriginal,
                    tamanhoBytes = novoAnexo.tamanhobytes,
                    contentType = novoAnexo.contenttype,
                    extensao = novoAnexo.extensao,
                    downloadUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao cadastrar anexo: {ex.Message}");
            }
        }

        /// <summary>Lista anexos de uma pesquisa (sem retornar o Base64 para não pesar).</summary>
        [HttpGet("pesquisa/{pesquisaId}")]
        public async Task<IActionResult> ListarPorPesquisa(int pesquisaId)
        {
            try
            {
                var lista = (await _repositoryAnexos.SelecionarTodosAsync())
                    .Where(a => a.pesquisaid == pesquisaId)
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
                        // Continua expondo um downloadUrl para reconstruir o arquivo sob demanda
                        downloadUrl = Url.Action(nameof(Download), "Anexos", new { id = a.anexoid }, Request.Scheme)
                    })
                    .ToList();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar anexos: {ex.Message}");
            }
        }

        /// <summary>Lista anexos de uma pergunta (sem Base64 no payload).</summary>
        [HttpGet("pergunta/{perguntaId}")]
        public async Task<IActionResult> ListarPorPergunta(int perguntaId)
        {
            try
            {
                var lista = (await _repositoryAnexos.SelecionarTodosAsync())
                    .Where(a => a.perguntaid == perguntaId)
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
                        downloadUrl = Url.Action(nameof(Download), "Anexos", new { id = a.anexoid }, Request.Scheme)
                    })
                    .ToList();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar anexos: {ex.Message}");
            }
        }

        /// <summary>
        /// Download de um anexo pelo ID.
        /// Agora reconstrói o arquivo a partir do Base64 salvo no banco (não lê do disco).
        /// </summary>
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var anexo = await _repositoryAnexos.SelecionarChaveAsync(id);
            if (anexo == null) return NotFound();

            if (string.IsNullOrWhiteSpace(anexo.base64data))
                return NotFound("Conteúdo Base64 não encontrado para este anexo.");

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(anexo.base64data);
            }
            catch
            {
                return StatusCode(500, "Falha ao decodificar o conteúdo Base64.");
            }

            var mime = string.IsNullOrWhiteSpace(anexo.contenttype)
                ? "application/octet-stream"
                : anexo.contenttype;

            var nomeDownload = !string.IsNullOrWhiteSpace(anexo.nomeoriginal)
                ? anexo.nomeoriginal
                : (anexo.nome + anexo.extensao);

            return File(bytes, mime, nomeDownload);
        }

        /// <summary>
        /// Exclui um anexo (apenas banco; não existe mais arquivo físico).
        /// </summary>
        [HttpDelete("ExcluirAnexo/{id}")]
        public async Task<IActionResult> ExcluirAnexo(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("ID inválido.");

                var anexo = await _repositoryAnexos.SelecionarChaveAsync(id);
                if (anexo == null) return NotFound("Anexo não encontrado.");

                // ❌ Não há mais arquivo físico para excluir
                await _repositoryAnexos.ExcluirAsync(anexo);
                return Ok("Anexo excluído com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir anexo: {ex.Message}");
            }
        }

        /// <summary>
        /// (Opcional) Obter metadados + Base64 em JSON para exibição inline (data URL no front).
        /// Útil quando quiser renderizar imagens diretamente sem novo round-trip.
        /// </summary>
        [HttpGet("{id}/inline")]
        public async Task<IActionResult> ObterInline(int id)
        {
            var anexo = await _repositoryAnexos.SelecionarChaveAsync(id);
            if (anexo == null) return NotFound();

            return Ok(new
            {
                anexo.anexoid,
                anexo.pesquisaid,
                anexo.perguntaid,
                anexo.nomeoriginal,
                anexo.contenttype,
                anexo.extensao,
                anexo.tamanhobytes,
                base64Data = anexo.base64data // ⚠️ cuidado com payload grande
            });
        }
    }
}
