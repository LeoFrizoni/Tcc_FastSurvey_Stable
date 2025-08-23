using FASTSURVEY.Dtos.Respostas;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using RespostaModel = SISTEMA_FASTSURVEY.MODEL.Models.Respostas;

namespace FASTSURVEY.Services.Resposta
{
    public class RespostaService : IRespostaService
    {
        private readonly FastSurveyContext _ctx;

        public RespostaService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<RespostaModel> CriarDiscursivaAsync(CriarRespostaDiscursivaRequest req, CancellationToken ct = default)
        {
            // valida pergunta
            var perguntaExiste = await _ctx.Perguntas
                .AsNoTracking()
                .AnyAsync(p => p.Perguntaid == req.PerguntaId, ct);

            if (!perguntaExiste)
                throw new InvalidOperationException("Pergunta n�o encontrada.");

            // valida pertencimento � pesquisa, se enviado
            if (req.PesquisaId.HasValue)
            {
                var pertence = await _ctx.Perguntas
                    .AsNoTracking()
                    .AnyAsync(p => p.Perguntaid == req.PerguntaId && p.Pesquisaid == req.PesquisaId.Value, ct);

                if (!pertence)
                    throw new InvalidOperationException("A pergunta informada n�o pertence � pesquisa enviada.");
            }

            var entity = new RespostaModel
            {
                Perguntaid = req.PerguntaId,
                Texto = (req.Texto ?? string.Empty).Trim(),
                Dataresposta = DateTime.UtcNow
            };

            _ctx.Respostas.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            return entity;
        }

        public async Task<RespostaModel> CriarOpcoesAsync(CriarRespostaOpcoesRequest req, CancellationToken ct = default)
        {
            if (req.OpcoesSelecionadas is null || req.OpcoesSelecionadas.Count == 0)
                throw new InvalidOperationException("Envie ao menos uma op��o selecionada.");

            var pergunta = await _ctx.Perguntas
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Perguntaid == req.PerguntaId, ct);

            if (pergunta is null)
                throw new InvalidOperationException("Pergunta n�o encontrada.");

            if (req.PesquisaId.HasValue)
            {
                var pertence = await _ctx.Perguntas
                    .AsNoTracking()
                    .AnyAsync(p => p.Perguntaid == req.PerguntaId && p.Pesquisaid == req.PesquisaId.Value, ct);

                if (!pertence)
                    throw new InvalidOperationException("A pergunta informada n�o pertence � pesquisa enviada.");
            }

            var opcoes = await _ctx.Opcoespergunta
                .Where(o => req.OpcoesSelecionadas.Contains(o.Opcaoid))
                .ToListAsync(ct);

            if (opcoes.Count != req.OpcoesSelecionadas.Count)
                throw new InvalidOperationException("Uma ou mais op��es n�o foram encontradas.");

            if (opcoes.Any(o => o.Perguntaid != req.PerguntaId))
                throw new InvalidOperationException("Foram enviadas op��es que n�o pertencem a essa pergunta.");

            var entity = new RespostaModel
            {
                Perguntaid = req.PerguntaId,
                Texto = null,
                Dataresposta = DateTime.UtcNow,
                Opcao = opcoes // N:N
            };

            _ctx.Respostas.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            return entity;
        }
    }
}
