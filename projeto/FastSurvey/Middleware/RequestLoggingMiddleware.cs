// using System.Text;

// namespace FASTSURVEY.Middleware
// {
//     public class RequestLoggingMiddleware
//     {
//         private readonly RequestDelegate _next;
//         private readonly ILogger<RequestLoggingMiddleware> _logger;

//         public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
//         {
//             _next = next;
//             _logger = logger;
//         }

//         public async Task InvokeAsync(HttpContext context)
//         {
//             // Log apenas para endpoints que começam com /api/pesquisas/
//             if (context.Request.Path.StartsWithSegments("/api/pesquisas"))
//             {
//                 _logger.LogInformation($"🔍 REQUEST: {context.Request.Method} {context.Request.Path}");
//                 _logger.LogInformation($"🔍 Content-Type: {context.Request.ContentType}");
//                 _logger.LogInformation($"🔍 Content-Length: {context.Request.ContentLength}");
                
//                 // Headers importantes
//                 foreach (var header in context.Request.Headers)
//                 {
//                     if (header.Key.Contains("Content") || header.Key.Contains("Authorization"))
//                     {
//                         var value = header.Key.Contains("Authorization") ? "[REDACTED]" : string.Join(", ", header.Value);
//                         _logger.LogInformation($"🔍 Header {header.Key}: {value}");
//                     }
//                 }

//                 // Se tem corpo, vamos ler
//                 if (context.Request.ContentLength > 0 && context.Request.Body.CanSeek)
//                 {
//                     context.Request.EnableBuffering();
//                     var body = await ReadRequestBodyAsync(context.Request);
//                     _logger.LogInformation($"🔍 Request Body: {body}");
                    
//                     // Reset stream position
//                     context.Request.Body.Position = 0;
//                 }
//             }

//             await _next(context);
//         }

//         private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
//         {
//             try
//             {
//                 using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
//                 var body = await reader.ReadToEndAsync();
//                 request.Body.Position = 0; // Reset position
//                 return body;
//             }
//             catch (Exception)
//             {
//                 return "[Could not read body]";
//             }
//         }
//     }
// }
