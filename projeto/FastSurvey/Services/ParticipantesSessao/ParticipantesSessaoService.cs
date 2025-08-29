#nullable enable
using FASTSURVEY.Dtos.ParticipantesSessao;
using FASTSURVEY.Services.Result;
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

        public async Task<ParticipanteResponse> ObterParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        )
        {
            var participante = await _participanteSessaoRepository.ObterParticipanteAsync(
                participanteId,
                ct
            );

            if (participante == null)
                return null!;

            return new ParticipanteResponse
            {
                ParticipanteId = participante.ParticipanteId,
                SessaoId = participante.SessaoId,
                NomeParticipante = participante.NomeParticipante,
                EntrouEm = participante.EntrouEm,
                SaiuEm = participante.SaiuEm,
                Ativo = participante.Ativo,
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
                    Ativo = p.Ativo,
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
                    Ativo = p.Ativo,
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
                Ativo = participante.Ativo,
            };
        }

        public async Task<bool> MarcarSaidaAsync(int participanteId, CancellationToken ct = default)
        {
            var result = await _participanteSessaoRepository.MarcarSaidaParticipanteAsync(
                participanteId,
                DateTime.UtcNow,
                ct
            );

            if (result)
                await _unitOfWork.CommitAsync(ct);

            return result;
        }

        public async Task<bool> AtualizarParticipanteAsync(
            int participanteId,
            ParticipanteUpdateRequest request,
            CancellationToken ct = default
        )
        {
            var result = await _participanteSessaoRepository.AtualizarParticipanteAsync(
                participanteId,
                request.NomeParticipante,
                ct
            );

            if (result)
                await _unitOfWork.CommitAsync(ct);

            return result;
        }

        public async Task<bool> RemoverParticipanteAsync(int participanteId, CancellationToken ct = default)
        {
            var result = await _participanteSessaoRepository.RemoverParticipanteAsync(
                participanteId,
                ct
            );

            if (result)
                await _unitOfWork.CommitAsync(ct);

            return result;
        }

        public async Task<List<ParticipanteResponse>> ObterParticipantesPorPeriodoAsync(
            string sessaoId,
            DateTime inicio,
            DateTime fim,
            CancellationToken ct = default
        )
        {
            var participantes = await _participanteSessaoRepository.ObterParticipantesPorPeriodoAsync(
                sessaoId,
                inicio,
                fim,
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
                    Ativo = p.Ativo,
                })
                .ToList();
        }

        public async Task<ServiceResult<bool>> AtivarParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        )
        {
            try
            {
                var result = await _participanteSessaoRepository.AtivarParticipanteAsync(
                    participanteId,
                    ct
                );

                if (result)
                    await _unitOfWork.CommitAsync(ct);

                return ServiceResult<bool>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao ativar participante: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResult<bool>> DesativarParticipanteAsync(
            int participanteId,
            CancellationToken ct = default
        )
        {
            try
            {
                var result = await _participanteSessaoRepository.DesativarParticipanteAsync(
                    participanteId,
                    ct
                );

                if (result)
                    await _unitOfWork.CommitAsync(ct);

                return ServiceResult<bool>.Ok(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Fail(
                    "ERROR",
                    $"Erro ao desativar participante: {ex.Message}"
                );
            }
        }
    }
}
