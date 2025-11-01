// using System.Diagnostics;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;

// namespace FASTSURVEY.Middleware
// {
//     public sealed class PerformanceOptions
//     {
//         public TimeSpan SlowRequestThreshold { get; set; } = TimeSpan.FromSeconds(2);
//     }

//     public sealed class PerformanceMiddleware
//     {
//         private readonly RequestDelegate _next;
//         private readonly ILogger<PerformanceMiddleware> _logger;
//         private readonly TimeSpan _threshold;

//         public PerformanceMiddleware(
//             RequestDelegate next,
//             ILogger<PerformanceMiddleware> logger,
//             IOptions<PerformanceOptions> options
//         )
//         {
//             _next = next;
//             _logger = logger;
//             _threshold = options.Value.SlowRequestThreshold;
//         }

//         public async Task InvokeAsync(HttpContext context)
//         {
//             var sw = Stopwatch.StartNew();

//             // garante headers no momento certo
//             context.Response.OnStarting(() =>
//             {
//                 context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;
//                 context.Response.Headers["X-Response-Time"] = sw.Elapsed.TotalMilliseconds.ToString(
//                     "F2"
//                 );
//                 return Task.CompletedTask;
//             });

//             try
//             {
//                 await _next(context);
//             }
//             catch (Exception ex)
//             {
//                 sw.Stop();
//                 _logger.LogError(
//                     ex,
//                     "Erro na requisição {Method} {Path} após {Elapsed}ms",
//                     context.Request.Method,
//                     context.Request.Path,
//                     sw.Elapsed.TotalMilliseconds
//                 );
//                 throw;
//             }

//             sw.Stop();
//             LogPerformance(context, sw.Elapsed);
//         }

//         private void LogPerformance(HttpContext ctx, TimeSpan elapsed)
//         {
//             var ms = elapsed.TotalMilliseconds;
//             var method = ctx.Request.Method;
//             var path = ctx.Request.Path;
//             var status = ctx.Response.StatusCode;

//             if (elapsed > _threshold)
//                 _logger.LogWarning(
//                     "Requisição lenta: {Method} {Path} {Elapsed}ms (Status {Status})",
//                     method,
//                     path,
//                     ms,
//                     status
//                 );
//             else if (ms > 1000)
//                 _logger.LogInformation(
//                     "Requisição demorada: {Method} {Path} {Elapsed}ms",
//                     method,
//                     path,
//                     ms
//                 );
//             else if (ms > 500)
//                 _logger.LogDebug(
//                     "Requisição moderada: {Method} {Path} {Elapsed}ms",
//                     method,
//                     path,
//                     ms
//                 );
//         }
//     }

//     public static class PerformanceMiddlewareExtensions
//     {
//         public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder app) =>
//             app.UseMiddleware<PerformanceMiddleware>();
//     }
// }
