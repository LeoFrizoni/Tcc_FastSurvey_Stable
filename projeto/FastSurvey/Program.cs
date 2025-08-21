using FASTSURVEY.Services;
using FASTSURVEY.Services.Login;
// —— novos usings ——
using FASTSURVEY.Services.Pesquisa;
using FASTSURVEY.Services.Pergunta;
using FASTSURVEY.Services.Opcoes;
using FASTSURVEY.Services.Tipos;
using FASTSURVEY.Services.Anexo;
using FASTSURVEY.Services.Pasta;
using FASTSURVEY.Services.Resposta;
using FASTSURVEY.Services.Anexo;
using FASTSURVEY.Services.Pasta;
using FASTSURVEY.Services.Resposta;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Opcional (caso esteja migrando para timestamptz e quer compatibilidade com timestamps antigos):
// AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ===== Connection String =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ===== DbContext =====
builder.Services.AddDbContext<FastSurveyContext>(opt =>
    opt.UseNpgsql(connectionString));

// ===== CORS (React) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
        // .AllowCredentials(); // habilite se for usar cookie HttpOnly p/ JWT
    });
});

// ===== Controllers + JSON =====
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        // evita ciclos de navegação do EF (Pergunta -> Opcoes -> Pergunta)
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // mantém os nomes conforme suas props minúsculas (sem camelCase forçado)
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
        // se precisar serializar enums como string:
        // o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FastSurvey API", Version = "v1" });

    // JWT no Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe o token JWT como: Bearer {seu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });

    // Se tiver OperationFilter para IFormFile, habilite:
    // options.OperationFilter<FASTSURVEY.IFormFileOperationFilter>();
});

// ===== Auth (JWT Bearer) =====
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "fastsurvey";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "fastsurvey.web";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TROQUE_POR_UMA_CHAVE_LONGA_E_SECRETA";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// ===== Kestrel/Form limits (upload de anexos/imagens) =====
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 256L * 1024L * 1024L; // 256 MB
});
builder.WebHost.ConfigureKestrel(k =>
{
    k.Limits.MaxRequestBodySize = 256L * 1024L * 1024L; // 256 MB
});

// ===== DI =====
// Genérico (repo base)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Repositórios específicos (interface -> implementação)
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IPesquisaRepository, PesquisaRepository>();
builder.Services.AddScoped<IPerguntaRepository, PerguntaRepository>();
builder.Services.AddScoped<IOpcaoPerguntaRepository, OpcaoPerguntaRepository>();
builder.Services.AddScoped<IRespostaRepository, RespostaRepository>();
builder.Services.AddScoped<IAnexoRepository, AnexoRepository>();
builder.Services.AddScoped<ITipoPerguntaRepository, TipoPerguntaRepository>();   // <— corrige o erro de DI
builder.Services.AddScoped<ITipoPesquisaRepository, TipoPesquisaRepository>();
builder.Services.AddScoped<ITipoUsuarioRepository, TipoUsuarioRepository>();

// (opcional) manter registros das classes concretas se houver injeção direta em algum lugar
builder.Services.AddScoped<LoginRepository>();
builder.Services.AddScoped<TokenRepository>();
builder.Services.AddScoped<PesquisaRepository>();
builder.Services.AddScoped<PerguntaRepository>();
builder.Services.AddScoped<OpcaoPerguntaRepository>();
builder.Services.AddScoped<RespostaRepository>();
builder.Services.AddScoped<AnexoRepository>();
builder.Services.AddScoped<TipoPerguntaRepository>();
builder.Services.AddScoped<TipoPesquisaRepository>();
builder.Services.AddScoped<TipoUsuarioRepository>();

// Serviços de domínio
builder.Services.AddScoped<ILoginService, LoginService>();

// —— novos serviços registrados ——
// TIPOS (usados pelo front p/ listar no modal e na criação)
builder.Services.AddScoped<ITipoPesquisaService, TipoPesquisaService>();
builder.Services.AddScoped<ITipoPerguntaService, TipoPerguntaService>();

// PESQUISA / PERGUNTA / OPÇÕES (criação, gabarito etc.)
builder.Services.AddScoped<IPesquisaService, PesquisaService>();
builder.Services.AddScoped<IPerguntaService, PerguntaService>();
builder.Services.AddScoped<IOpcaoPerguntaService, OpcaoPerguntaService>();
// ANEXO
builder.Services.AddScoped<IAnexoService, AnexoService>();
// PASTA
builder.Services.AddScoped<IPastaService, PastaService>();
// RESPOSTA
builder.Services.AddScoped<IRespostaService, RespostaService>();
// TOKEN
builder.Services.AddScoped<FASTSURVEY.Services.Tokens.ITokenService, FASTSURVEY.Services.Tokens.TokenService>();

var app = builder.Build();

// ===== Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirReact");

app.UseHttpsRedirection();

app.UseAuthentication();   // antes de Authorization
app.UseAuthorization();

app.MapControllers();
app.Run();
