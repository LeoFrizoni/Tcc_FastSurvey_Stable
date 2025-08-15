using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// String de conexão com PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configura o DbContext
builder.Services.AddDbContext<FastSurveyContext>(opt => opt.UseNpgsql(connectionString));

// Configura CORS para permitir o front-end em React
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirReact", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:3001"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Habilita Swagger em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplica CORS antes da autorização
app.UseCors("PermitirReact");

app.UseAuthorization();

app.MapControllers();

app.Run();
