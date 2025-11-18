using System.Threading.Tasks;

namespace CRM.Infrastructure.Logging;

public interface IAuditLogger
{
    Task LogAsync(string eventName, object data);
}
