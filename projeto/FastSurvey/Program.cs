using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FASTSURVEY;
using FASTSURVEY.Middleware;
using FASTSURVEY.Services.Cache;
using FASTSURVEY.Services.Mobile;
using FASTSURVEY.Services.PesquisaInterativa;
using FASTSURVEY.Services.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Controllers & JSON ----------
builder
    .Services.AddControllers(o => o.SuppressAsyncSuffixInActionNames = false)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        o.JsonSerializerOptions.WriteIndented = false;
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ---------- Compression ----------
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
    o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/problem+json", "text/plain", "image/svg+xml" }
    );
});

// ---------- Cache ----------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddResponseCaching(o =>
{
    o.MaximumBodySize = 64 * 1024 * 1024; // 64MB
    o.SizeLimit = 100 * 1024 * 1024; // 100MB
});
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("Cache"));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

// ---------- Validation & Mobile & Interativa ----------
builder.Services.Configure<ValidationOptions>(builder.Configuration.GetSection("Validation"));
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IMobileService, MobileService>();
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();

// ---------- Performance Monitoring ----------
builder.Services.Configure<PerformanceOptions>(builder.Configuration.GetSection("Performance"));

// ---------- Rate Limiting ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    // Política específica para login/reset
    options.AddFixedWindowLimiter(
        "strict-login",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.AutoReplenishment = true;
            opt.QueueLimit = 0;
        }
    );

    // Limite global
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var key =
            ctx.User?.Identity?.IsAuthenticated == true
                ? (ctx.User.Identity?.Name ?? ctx.Request.Headers.Host.ToString())
                : (ctx.Connection.RemoteIpAddress?.ToString() ?? "anon");
        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ =>
                new()
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    AutoReplenishment = true,
                }
        );
    });
});

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FastSurvey API", Version = "v1" });
    c.AddSecurityDefinition(
        "Bearer",
        new()
        {
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Envie: Bearer {seu_token}",
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            BearerFormat = "JWT",
            Scheme = "Bearer",
        }
    );
    c.AddSecurityRequirement(
        new()
        {
            {
                new()
                {
                    Reference = new()
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// ---------- CORS (restringido às origens do config) ----------
var allowedOrigins =
    builder.Configuration.GetSection("Google:AuthorizedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(o =>
{
    o.AddPolicy(
        "DefaultCors",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders(
                    "X-Pagination",
                    "X-Total-Count",
                    "X-Response-Time",
                    "X-Request-Id"
                );
        }
    );
});

// ---------- JWT/Auth ----------
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default-key-for-development";
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("PremiumOrAdmin", p => p.RequireRole("Premium", "Admin"));
});

// ---------- FastSurvey Services ----------
builder.Services.AddFastSurvey(builder.Configuration);

var app = builder.Build();

// ---------- Erros & Segurança ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// arquivos estáticos (wwwroot/uploads/avatars etc.)
app.UseStaticFiles(
    new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=86400";
        },
    }
);

app.UseCors("DefaultCors");
app.UseResponseCaching();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// middlewares custom
app.UsePerformanceMonitoring();
app.UseRoleAuthorization();
app.UseAuthorizationMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
