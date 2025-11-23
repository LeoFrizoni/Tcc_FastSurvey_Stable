#nullable enable
using System;
using System.Security.Cryptography;
using FASTSURVEY.Dtos.ParticipantesSessao;
using FASTSURVEY.Services.Result;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.ParticipantesSessao
{
    public class ParticipantesSessaoService : IParticipantesSessaoService
    {
        private readonly IParticipanteSessaoRepository _participanteSessaoRepository;
        private readonly ISessaoInterativaRepository _sessaoInterativaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ParticipantesSessaoService(
            IParticipanteSessaoRepository participanteSessaoRepository,
            ISessaoInterativaRepository sessaoInterativaRepository,
            IUnitOfWork unitOfWork
        )
        {
            _participanteSessaoRepository = participanteSessaoRepository;
            _sessaoInterativaRepository = sessaoInterativaRepository;
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
            var sessaoId = (request.SessaoId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(sessaoId))
                throw new ArgumentException("O ID da sessão é obrigatório.");

            await GarantirSessaoAsync(sessaoId, request.PesquisaId, ct);

            var participante = await _participanteSessaoRepository.RegistrarEntradaAsync(
                sessaoId,
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

        private async Task<SessoesInterativas> GarantirSessaoAsync(
            string sessaoId,
            int? pesquisaId,
            CancellationToken ct
        )
        {
            var existente = await _sessaoInterativaRepository.FirstOrDefaultAsync(
                s => s.SessaoId == sessaoId,
                ct
            );

            if (existente is not null)
                return existente;

            if (!pesquisaId.HasValue || pesquisaId.Value <= 0)
                throw new ArgumentException(
                    "Sessão inexistente. Informe o ID da pesquisa para continuar."
                );

            var novaSessao = new SessoesInterativas
            {
                SessaoId = sessaoId,
                PesquisaId = pesquisaId.Value,
                CodigoAcesso = await GerarCodigoAcessoUnicoAsync(ct),
                CriadaEm = DateTime.UtcNow,
                Ativa = true,
                PerguntaAtiva = false,
                ModoApresentacao = false,
            };

            await _sessaoInterativaRepository.AddAsync(novaSessao, ct);
            return novaSessao;
        }

        private async Task<string> GerarCodigoAcessoUnicoAsync(CancellationToken ct)
        {
            const int tentativasMax = 6;

            for (var tentativa = 0; tentativa < tentativasMax; tentativa++)
            {
                var codigo = GerarCodigoAcesso();
                var existe = await _sessaoInterativaRepository.ExistsAsync(
                    s => s.CodigoAcesso == codigo,
                    ct
                );

                if (!existe)
                    return codigo;
            }

            throw new InvalidOperationException(
                "Não foi possível gerar um código de acesso único para a sessão."
            );
        }

        private static string GerarCodigoAcesso()
        {
            const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Span<char> buffer = stackalloc char[8];
            Span<byte> bytes = stackalloc byte[buffer.Length];
            RandomNumberGenerator.Fill(bytes);

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = alfabeto[bytes[i] % alfabeto.Length];
            }

            return new string(buffer);
        }
    }
}
