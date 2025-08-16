using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System.IO;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnexosController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryAnexos _repositoryAnexos;
        private readonly string _caminhoArquivos;

        public AnexosController(FastSurveyContext context)
        {
            _context = context;
            _repositoryAnexos = new RepositoryAnexos(_context, true);
            _caminhoArquivos = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            // Cria a pasta Uploads se não existir
            if (!Directory.Exists(_caminhoArquivos))
            {
                Directory.CreateDirectory(_caminhoArquivos);
            }
        }

        // Upload de anexo vinculado à pesquisa
    [HttpPost("{pesquisaId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CadastrarAnexo(int pesquisaId, [FromForm] IFormFile anexo)
        {
            try
            {
                if (anexo == null || anexo.Length == 0)
                    return BadRequest("Anexo não pode ser nulo ou vazio.");

                var caminhoCompleto = Path.Combine(_caminhoArquivos, anexo.FileName);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await anexo.CopyToAsync(stream);
                }

                var novoAnexo = new anexos
                {
                    nome = Path.GetFileNameWithoutExtension(anexo.FileName),
                    extensao = Path.GetExtension(anexo.FileName),
                    pesquisaid = pesquisaId
                };

                await _repositoryAnexos.IncluirAsync(novoAnexo);

                return Ok("Anexo cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao cadastrar anexo: {ex.Message}");
            }
        }

        [HttpGet("ListarAnexos")]
        public async Task<IActionResult> ListarAnexos()
        {
            try
            {
                var anexos = await _repositoryAnexos.SelecionarTodosAsync();
                return Ok(anexos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar anexos: {ex.Message}");
            }
        }

        [HttpGet("SelecionarAnexosPorId/{id}")]
        public async Task<IActionResult> ListarAnexosPorId(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("ID inválido.");

                var anexo = await _repositoryAnexos.SelecionarChaveAsync(id);
                if (anexo == null)
                    return NotFound("Anexo não encontrado.");

                return Ok(anexo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar anexo por ID: {ex.Message}");
            }
        }

        [HttpPut("AlterarAnexoPorId/{id}")]
        public async Task<IActionResult> AlterarAnexoPorId(int id, [FromBody] anexos anexo)
        {
            try
            {
                if (id <= 0 || anexo == null)
                    return BadRequest("Dados inválidos.");

                var anexoExistente = await _repositoryAnexos.SelecionarChaveAsync(id);
                if (anexoExistente == null)
                    return NotFound("Anexo não encontrado.");

                await _repositoryAnexos.AlterarAsync(anexo);
                return Ok("Anexo alterado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao alterar anexo: {ex.Message}");
            }
        }

        [HttpDelete("ExcluirAnexo/{id}")]
        public async Task<IActionResult> ExcluirAnexo(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("ID inválido.");

                var anexo = await _repositoryAnexos.SelecionarChaveAsync(id);
                if (anexo == null)
                    return NotFound("Anexo não encontrado.");

                // Deletar arquivo físico
                var caminhoArquivo = Path.Combine(_caminhoArquivos, anexo.nome + anexo.extensao);
                if (System.IO.File.Exists(caminhoArquivo))
                {
                    System.IO.File.Delete(caminhoArquivo);
                }

                await _repositoryAnexos.ExcluirAsync(anexo);
                return Ok("Anexo excluído com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir anexo: {ex.Message}");
            }
        }
    }
}