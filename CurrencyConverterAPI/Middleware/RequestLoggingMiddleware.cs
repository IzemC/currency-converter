public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        
        // Capture client IP
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        
        // Capture client ID from JWT
        var clientId = context.User.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value ?? "anonymous";
        
        try
        {
            await _next(context);
            
            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            _logger.LogInformation("Request {Method} {Path} from {ClientIp} (ClientId: {ClientId}) responded {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                clientIp,
                clientId,
                context.Response.StatusCode,
                elapsedMs);
        }
        catch (Exception ex)
        {
            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            _logger.LogError(ex, "Request {Method} {Path} from {ClientIp} (ClientId: {ClientId}) failed in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                clientIp,
                clientId,
                elapsedMs);
            
            throw;
        }
    }
}