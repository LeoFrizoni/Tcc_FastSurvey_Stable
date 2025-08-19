using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Conn string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext
builder.Services.AddDbContext<FastSurveyContext>(opt => opt.UseNpgsql(connectionString));

// CORS p/ React
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<FASTSURVEY.IFormFileOperationFilter>();
});
builder.Services.AddEndpointsApiExplorer();

// ===== DI dos repositórios/serviços =====
// você já tinha:
builder.Services.AddScoped<RepositoryLogin>();
builder.Services.AddScoped<FASTSURVEY.Services.ServicePesquisas>();

// adicione estes (usados nos controllers que criamos/ajustamos):
builder.Services.AddScoped<RepositoryTipoUsuario>();
builder.Services.AddScoped<RepositoryTipoPesquisa>();   // se existir controller de TipoPesquisa
// (se seus outros controllers injetarem repositórios concretos, registre-os aqui também)

// ========================================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirReact");

app.UseAuthorization();

app.MapControllers();

app.Run();
