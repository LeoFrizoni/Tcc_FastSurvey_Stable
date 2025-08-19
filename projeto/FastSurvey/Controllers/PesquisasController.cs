// FASTSURVEY/Controllers/PesquisasController.cs
using FASTSURVEY.Models;
using FASTSURVEY.Models.DTO;
using FASTSURVEY.Services;
using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;   // <- para o tipo 'pesquisas'
using System;
using System.Collections.Generic;        // <- para List<>
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")] // => /api/pesquisas
    [ApiController]
    [Produces("application/json")]
    public class PesquisasController : ControllerBase
    {
    // ...existing code...

        /// <summary>
        /// Move uma pesquisa para outra pasta.
        /// </summary>
        // PATCH: api/pesquisas/{id}/mover-pasta
        [HttpPatch("{id:int}/mover-pasta")]
        public async Task<IActionResult> MoverPasta(int id, [FromBody] int novaPastaId, CancellationToken ct)
        {
            if (id <= 0 || novaPastaId <= 0)
                return BadRequest("ID da pesquisa e da pasta devem ser válidos.");

            var sucesso = await _svcPesquisas.MoverPesquisaParaPastaAsync(id, novaPastaId, ct);
            if (!sucesso)
                return NotFound("Pesquisa ou pasta não encontrada.");

            return NoContent();
        }
        private readonly ServicePesquisas _svcPesquisas;

        public PesquisasController(ServicePesquisas svcPesquisas)
        {
            _svcPesquisas = svcPesquisas;
        }

        // GET: api/Pesquisas
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _svcPesquisas.ListarTodasPesquisasAsync(ct);
            if (list == null || list.Count == 0) return NotFound("Nenhuma pesquisa encontrada.");
            return Ok(list);
        }

        // GET: api/Pesquisas/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _svcPesquisas.BuscarPesquisaPorIdAsync(id, ct);
            if (result is { } && result.GetType().GetProperty("mensagem") != null) return NotFound(result);
            return Ok(result);
        }

        // GET: api/pesquisas/usuario/16
        // Retorna [] quando não houver pesquisas (evita erro no front)
        [HttpGet("usuario/{id:int}")]
        public async Task<IActionResult> ListarPorUsuario(int id, CancellationToken ct)
        {
            if (id <= 0) return BadRequest("ID de usuário inválido.");
            try
            {
                var list = await _svcPesquisas.ListarPorUsuarioAsync(id, ct);
                return Ok(list ?? new List<pesquisas>());
            }
            catch (Exception ex)
            {
                // Log detalhado (pode ser substituído por um logger real)
                Console.WriteLine($"Erro ao buscar pesquisas do usuário {id}: {ex.Message}\n{ex.StackTrace}");
                return Problem($"Erro interno ao buscar pesquisas do usuário: {ex.Message}");
            }
        }

        // POST: api/Pesquisas
        // Body: PesquisaVM (com perguntas dentro)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PesquisaVM vm, CancellationToken ct)
        {
            try
            {
                var created = await _svcPesquisas.CadastrarPesquisaAsync(vm, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.pesquisaid }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao cadastrar pesquisa: {ex.Message}");
            }
        }

        // PUT: api/Pesquisas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PesquisaVM vm, CancellationToken ct)
        {
            try
            {
                vm.CodigoPesquisa = id;
                var updated = await _svcPesquisas.AtualizarPesquisaAsync(vm, ct);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao atualizar: {ex.Message}");
            }
        }

        // DELETE: api/Pesquisas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _svcPesquisas.ExcluirPesquisaAsync(id, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem($"Erro ao excluir: {ex.Message}");
            }
        }

        /// <summary>
    }
}
