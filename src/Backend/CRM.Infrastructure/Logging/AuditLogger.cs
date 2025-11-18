using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Logging;

public class AuditLogger : IAuditLogger
{
    public Task LogAsync(string eventName, object data)
    {
        var payload = JsonSerializer.Serialize(data);
        Console.WriteLine($"[AUDIT] {DateTime.UtcNow:o} {eventName} {payload}");
        return Task.CompletedTask;
    }
}
