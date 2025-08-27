using FASTSURVEY.Dtos.ParticipantesSessao;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.ParticipantesSessao
{
    public class ParticipantesSessaoService : IParticipantesSessaoService
    {
        private readonly IParticipanteSessaoRepository _participanteSessaoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ParticipantesSessaoService(
            IParticipanteSessaoRepository participanteSessaoRepository,
            IUnitOfWork unitOfWork
        )
        {
            _participanteSessaoRepository = participanteSessaoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ParticipanteResponse?> ObterPorNomeESessaoAsync(
            string nome,
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var participante = await _participanteSessaoRepository.ObterPorNomeESessaoAsync(
                nome,
                sessaoId,
                ct
            );

            if (participante == null)
                return null;

            return new ParticipanteResponse
            {
                ParticipanteId = participante.ParticipanteId,
                SessaoId = participante.SessaoId,
                NomeParticipante = participante.NomeParticipante,
                EntrouEm = participante.EntrouEm,
                SaiuEm = participante.SaiuEm,
                PontuacaoTotal = participante.PontuacaoTotal,
                RespostasCorretas = participante.RespostasCorretas,
                TempoMedioResposta = participante.TempoMedioResposta,
                Ranking = participante.Ranking,
            };
        }

        public async Task<List<ParticipanteResponse>> ObterParticipantesAtivosAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var participantes = await _participanteSessaoRepository.ObterParticipantesAtivosAsync(
                sessaoId,
                ct
            );

            return participantes
                .Select(p => new ParticipanteResponse
                {
                    ParticipanteId = p.ParticipanteId,
                    SessaoId = p.SessaoId,
                    NomeParticipante = p.NomeParticipante,
                    EntrouEm = p.EntrouEm,
                    SaiuEm = p.SaiuEm,
                    PontuacaoTotal = p.PontuacaoTotal,
                    RespostasCorretas = p.RespostasCorretas,
                    TempoMedioResposta = p.TempoMedioResposta,
                    Ranking = p.Ranking,
                })
                .ToList();
        }

        public async Task<int> ContarParticipantesAtivosAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            return await _participanteSessaoRepository.ContarParticipantesAtivosAsync(sessaoId, ct);
        }

        public async Task<List<ParticipanteResponse>> ObterParticipantesPorSessaoAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var participantes =
                await _participanteSessaoRepository.ObterParticipantesPorSessaoAsync(sessaoId, ct);

            return participantes
                .Select(p => new ParticipanteResponse
                {
                    ParticipanteId = p.ParticipanteId,
                    SessaoId = p.SessaoId,
                    NomeParticipante = p.NomeParticipante,
                    EntrouEm = p.EntrouEm,
                    SaiuEm = p.SaiuEm,
                    PontuacaoTotal = p.PontuacaoTotal,
                    RespostasCorretas = p.RespostasCorretas,
                    TempoMedioResposta = p.TempoMedioResposta,
                    Ranking = p.Ranking,
                })
                .ToList();
        }

        public async Task<ParticipanteResponse> RegistrarEntradaAsync(
            ParticipanteRequest request,
            CancellationToken ct = default
        )
        {
            var participante = await _participanteSessaoRepository.RegistrarEntradaAsync(
                request.SessaoId,
                request.NomeParticipante,
                DateTime.UtcNow,
                ct
            );
            
            await _unitOfWork.CommitAsync(ct);

            return new ParticipanteResponse
            {
                ParticipanteId = participante.ParticipanteId,
                SessaoId = participante.SessaoId,
                NomeParticipante = participante.NomeParticipante,
                EntrouEm = participante.EntrouEm,
                SaiuEm = participante.SaiuEm,
                PontuacaoTotal = participante.PontuacaoTotal,
                RespostasCorretas = participante.RespostasCorretas,
                TempoMedioResposta = participante.TempoMedioResposta,
                Ranking = participante.Ranking,
            };
        }

        public async Task<bool> MarcarSaidaAsync(int participanteId, CancellationToken ct = default)
        {
            var result = await _participanteSessaoRepository.MarcarSaidaParticipanteAsync(
                participanteId,
                DateTime.UtcNow,
                ct
            );
            await _unitOfWork.CommitAsync(ct);
            return result > 0;
        }

        public async Task<bool> MarcarSaidaTodosAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var result = await _participanteSessaoRepository.MarcarSaidaParticipantesAsync(
                sessaoId,
                DateTime.UtcNow,
                ct
            );
            await _unitOfWork.CommitAsync(ct);
            return result > 0;
        }

        public async Task<List<ParticipanteRankingResponse>> ObterRankingAsync(
            string sessaoId,
            CancellationToken ct = default
        )
        {
            var participantes =
                await _participanteSessaoRepository.ObterParticipantesPorSessaoAsync(sessaoId, ct);

            var ranking = participantes
                .Where(p => p.Ranking.HasValue)
                .OrderBy(p => p.Ranking)
                .Select(p => new ParticipanteRankingResponse
                {
                    ParticipanteId = p.ParticipanteId,
                    NomeParticipante = p.NomeParticipante,
                    PontuacaoTotal = p.PontuacaoTotal,
                    RespostasCorretas = p.RespostasCorretas,
                    TempoMedioResposta = p.TempoMedioResposta,
                    Ranking = p.Ranking ?? 0,
                })
                .ToList();

            return ranking;
        }

        public async Task<ParticipanteResponse?> ObterPorIdAsync(
            int participanteId,
            CancellationToken ct = default
        )
        {
            var participante = await _participanteSessaoRepository.GetByIdAsync(participanteId, ct);

            if (participante == null)
                return null;

            return new ParticipanteResponse
            {
                ParticipanteId = participante.ParticipanteId,
                SessaoId = participante.SessaoId,
                NomeParticipante = participante.NomeParticipante,
                EntrouEm = participante.EntrouEm,
                SaiuEm = participante.SaiuEm,
                PontuacaoTotal = participante.PontuacaoTotal,
                RespostasCorretas = participante.RespostasCorretas,
                TempoMedioResposta = participante.TempoMedioResposta,
                Ranking = participante.Ranking,
            };
        }
    }
}
