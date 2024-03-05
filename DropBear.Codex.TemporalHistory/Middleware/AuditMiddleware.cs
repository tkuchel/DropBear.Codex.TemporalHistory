using DropBear.Codex.TemporalHistory.Services;

namespace DropBear.Codex.TemporalHistory.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, MyDbContext dbContext, AuditService auditService)
    {
        // Logic before request processing
        // Possibly start tracking changes or prepare the audit service

        await _next(context); // Process the request

        // Logic after request processing
        var auditEntries = dbContext.OnBeforeSaveChanges();
        auditService.SaveAuditEntries(auditEntries);
        dbContext.SaveChanges(); // Ensure to capture audit entries
    }
}

// In Startup.cs or Program.cs
// app.UseMiddleware<AuditMiddleware>();

