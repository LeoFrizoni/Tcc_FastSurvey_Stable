// ===== Services da API =====
using FASTSURVEY.Services.Analytics;
using FASTSURVEY.Services.Anexo;
using FASTSURVEY.Services.Blocklist;
using FASTSURVEY.Services.Email;
using FASTSURVEY.Services.Export;
using FASTSURVEY.Services.ExternalLogins;
using FASTSURVEY.Services.FeatureFlags;
using FASTSURVEY.Services.Login; // ILoginService, LoginService, IAvatarService, AvatarService
using FASTSURVEY.Services.Opcoes;
using FASTSURVEY.Services.ParticipantesSessao;
using FASTSURVEY.Services.Pasta;
using FASTSURVEY.Services.Pergunta;
using FASTSURVEY.Services.Pesquisa;
using FASTSURVEY.Services.PesquisaInterativa;
using FASTSURVEY.Services.PesquisaPasta;
using FASTSURVEY.Services.QRCode;
using FASTSURVEY.Services.Resposta;
// Novos Services
using FASTSURVEY.Services.Resultados;
using FASTSURVEY.Services.Security;
using FASTSURVEY.Services.Tipos;
using FASTSURVEY.Services.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// ===== Camada de dados (MODEL) =====
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFastSurvey(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            // DbContext
            services.AddDbContext<FastSurveyContext>(opt =>
                opt.UseNpgsql(config.GetConnectionString("DefaultConnection"))
            );

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ===== Repositório Genérico =====
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // ===== Repositories (MODEL) =====
            services.AddScoped<ILoginRepository, LoginRepository>();
            services.AddScoped<ITipoUsuarioRepository, TipoUsuarioRepository>();
            services.AddScoped<ITipoPesquisaRepository, TipoPesquisaRepository>();
            services.AddScoped<ITipoPerguntaRepository, TipoPerguntaRepository>();
            services.AddScoped<IPesquisaRepository, PesquisaRepository>();
            services.AddScoped<IPesquisaPastaRepository, PesquisaPastaRepository>();
            services.AddScoped<IPerguntaRepository, PerguntaRepository>();
            services.AddScoped<IOpcaoPerguntaRepository, OpcaoPerguntaRepository>();
            services.AddScoped<IRespostaRepository, RespostaRepository>();
            services.AddScoped<IPastaRepository, PastaRepository>();
            services.AddScoped<IAnexoRepository, AnexoRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<ILoginAvatarRepository, LoginAvatarRepository>();
            services.AddScoped<IExternalLoginRepository, ExternalLoginRepository>();
            services.AddScoped<ISessaoInterativaRepository, SessaoInterativaRepository>();
            services.AddScoped<IParticipanteSessaoRepository, ParticipanteSessaoRepository>();
            services.AddScoped<IBlocklistRepository, BlocklistRepository>();
            services.AddScoped<IFeatureFlagsRepository, FeatureFlagsRepository>();

            // ===== Services (API) =====
            services.AddScoped<IAnexoService, AnexoService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IAvatarService, AvatarService>();
            services.AddScoped<IOpcaoPerguntaService, OpcaoPerguntaService>();
            services.AddScoped<IPastaService, PastaService>();
            services.AddScoped<IPerguntaService, PerguntaService>();
            services.AddScoped<IPesquisaService, PesquisaService>();
            services.AddScoped<IPesquisaPastaService, PesquisaPastaService>();
            services.AddScoped<IRespostaService, RespostaService>();
            services.AddScoped<ITipoPerguntaService, TipoPerguntaService>();
            services.AddScoped<ITipoPesquisaService, TipoPesquisaService>();
            services.AddScoped<ITipoUsuarioService, TipoUsuarioService>();
            services.AddScoped<ITokenService, TokenService>();

            // ===== Novos Services =====
            services.AddScoped<IResultadosService, ResultadosService>();
            services.AddScoped<IPesquisaInterativaService, PesquisaInterativaService>();
            services.AddScoped<IQRCodeService, QRCodeService>();
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            // ===== Services de Segurança e Controle =====
            services.AddScoped<IBlocklistService, BlocklistService>();
            services.AddScoped<IExternalLoginsService, ExternalLoginsService>();
            services.AddScoped<IFeatureFlagsService, FeatureFlagsService>();
            services.AddScoped<IParticipantesSessaoService, ParticipantesSessaoService>();

            // ===== Email Service =====
            services.AddScoped<IEmailSender, SmtpEmailSender>();

            return services;
        }
    }
}
