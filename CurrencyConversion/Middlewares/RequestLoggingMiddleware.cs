using CurrencyConversion.Controllers;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestDelegate> _logger;
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestDelegate> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var startTime = DateTime.UtcNow;

        // Retrieve the client IP and ClientId from JWT if available.
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var clientId = context.User?.FindFirst("clientId")?.Value ?? "unknown";

        // Process the request.
        await _next(context);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        var statusCode = context.Response.StatusCode;
        var method = context.Request.Method;
        var endpoint = context.Request.Path;

        _logger.LogInformation("Request from {ClientIp} (ClientId: {ClientId}) {Method} {Endpoint} responded {StatusCode} in {Elapsed:0.0000} ms", clientIp, clientId, method, endpoint, statusCode, elapsedMs);
    }
}
