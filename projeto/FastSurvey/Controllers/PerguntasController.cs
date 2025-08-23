using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Dtos.Perguntas;
using FASTSURVEY.Dtos.Perguntas.Discursiva;
using FASTSURVEY.Dtos.Perguntas.Objetiva;
using FASTSURVEY.Dtos.Perguntas.Multipla;

using FASTSURVEY.Services.Pergunta;
using Microsoft.AspNetCore.Mvc;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerguntasController : ControllerBase
    {
        private readonly IPerguntaService _service;

        public PerguntasController(IPerguntaService service)
        {
            _service = service;
        }

        // -------------------- Helpers --------------------
        private IActionResult ToActionResult<T>(ServiceResult<T> result)
        {
            if (result == null) return StatusCode(500, "Erro inesperado.");

            if (result.Success) return Ok(result.Data);

            // Ajuste para acessar a lista de erros corretamente
            return BadRequest(result.Errors);
        }

        // -------------------- CREATE --------------------

        [HttpPost]
        public async Task<IActionResult> Criar(
            [FromBody] CriarPerguntaRequest req,
            CancellationToken ct)
        {
            var result = await _service.CriarAsync(req, ct);
            return ToActionResult(result);
        }

        [HttpPost("discursiva")]
        public async Task<IActionResult> CriarDiscursiva(
            [FromBody] CriarPerguntaDiscursivaRequest req,
            CancellationToken ct)
        {
            var result = await _service.CriarDiscursivaAsync(req, ct);
            return ToActionResult(result);
        }

        [HttpPost("objetiva")]
        public async Task<IActionResult> CriarObjetiva(
            [FromBody] CriarPerguntaObjetivaRequest req,
            CancellationToken ct)
        {
            var result = await _service.CriarObjetivaAsync(req, ct);
            return ToActionResult(result);
        }

        [HttpPost("multipla")]
        public async Task<IActionResult> CriarMultipla(
            [FromBody] CriarPerguntaMultiplaRequest req,
            CancellationToken ct)
        {
            var result = await _service.CriarMultiplaAsync(req, ct);
            return ToActionResult(result);
        }

        // -------------------- UPDATE --------------------

        [HttpPut("discursiva")]
        public async Task<IActionResult> AtualizarDiscursiva(
            [FromBody] AtualizarPerguntaDiscursivaRequest req,
            CancellationToken ct)
        {
            var result = await _service.AtualizarDiscursivaAsync(req, ct);
            return ToActionResult(result);
        }

        [HttpPut("objetiva")]
        public async Task<IActionResult> AtualizarObjetiva(
            [FromBody] AtualizarPerguntaObjetivaRequest req,
            CancellationToken ct)
        {
            var result = await _service.AtualizarObjetivaAsync(req, ct);
            return ToActionResult(result);
        }

        [HttpPut("multipla")]
        public async Task<IActionResult> AtualizarMultipla(
            [FromBody] AtualizarPerguntaMultiplaRequest req,
            CancellationToken ct)
        {
            var result = await _service.AtualizarMultiplaAsync(req, ct);
            return ToActionResult(result);
        }

        // -------------------- DELETE --------------------

        [HttpDelete("{perguntaId:int}")]
        public async Task<IActionResult> Excluir(int perguntaId, CancellationToken ct)
        {
            var result = await _service.ExcluirAsync(perguntaId, ct);
            return ToActionResult(result);
        }

        // -------------------- GET --------------------

        [HttpGet("{perguntaId:int}")]
        public async Task<IActionResult> ObterPorId(int perguntaId, CancellationToken ct)
        {
            var result = await _service.ObterPorIdAsync(perguntaId, ct);
            return ToActionResult(result);
        }

        [HttpGet("por-pesquisa/{pesquisaId:int}")]
        public async Task<IActionResult> ListarPorPesquisa(int pesquisaId, CancellationToken ct)
        {
            var result = await _service.ListarPorPesquisaAsync(pesquisaId, ct);
            return ToActionResult(result);
        }

        // -------------------- GABARITO --------------------
        public class DefinirGabaritoBody
        {
            public IEnumerable<int> OpcaoIds { get; set; } = new List<int>();
            public bool PermitirApenasUma { get; set; } = true; // true=Objetiva; false=M�ltipla
        }

        [HttpPost("{perguntaId:int}/gabarito")]
        public async Task<IActionResult> DefinirGabarito(
            int perguntaId,
            [FromBody] DefinirGabaritoBody body,
            CancellationToken ct)
        {
            var result = await _service.DefinirGabaritoAsync(perguntaId, body.OpcaoIds, body.PermitirApenasUma, ct);
            return ToActionResult(result);
        }

        // -------------------- ORDENAR PERGUNTAS --------------------
        public class ReordenarPerguntasBody
        {
            public int PesquisaId { get; set; }
            public IReadOnlyList<int> PerguntaIdsNaOrdem { get; set; } = new List<int>();
        }

        [HttpPost("reordenar")]
        public async Task<IActionResult> ReordenarPerguntas(
            [FromBody] ReordenarPerguntasBody body,
            CancellationToken ct)
        {
            var result = await _service.ReordenarAsync(body.PesquisaId, body.PerguntaIdsNaOrdem, ct);
            return ToActionResult(result);
        }

        // -------------------- OP��ES --------------------

        [HttpPost("{perguntaId:int}/opcoes")]
        public async Task<IActionResult> AdicionarOpcao(
            int perguntaId,
            [FromBody] OpcaoPerguntaRequest req,
            CancellationToken ct)
        {
            var result = await _service.AdicionarOpcaoAsync(perguntaId, req, ct);
            return ToActionResult(result);
        }

        [HttpPut("opcoes")]
        public async Task<IActionResult> AtualizarOpcao(
            [FromBody] OpcaoPerguntaUpdateRequest req,
            CancellationToken ct)
        {
            var result = await _service.AtualizarOpcaoAsync(req, ct);
            return ToActionResult(result);
        }

        [HttpDelete("opcoes/{opcaoId:int}")]
        public async Task<IActionResult> RemoverOpcao(int opcaoId, CancellationToken ct)
        {
            // O modelo n�o suporta soft-delete; par�metro est� na interface, mas sempre remover� f�sico
            var result = await _service.RemoverOpcaoAsync(opcaoId, softDelete: false, ct);
            return ToActionResult(result);
        }

        // Como seu modelo de OpcaoPergunta n�o possui coluna de ordena��o,
        // este endpoint invocar� o service que retorna "n�o suportado".
        public class ReordenarOpcoesBody
        {
            public IReadOnlyList<int> OpcaoIdsNaOrdem { get; set; } = new List<int>();
        }

        [HttpPost("{perguntaId:int}/opcoes/reordenar")]
        public async Task<IActionResult> ReordenarOpcoes(
            int perguntaId,
            [FromBody] ReordenarOpcoesBody body,
            CancellationToken ct)
        {
            var result = await _service.ReordenarOpcoesAsync(perguntaId, body.OpcaoIdsNaOrdem, ct);
            return ToActionResult(result);
        }
    }
}
