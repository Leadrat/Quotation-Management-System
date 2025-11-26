using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Domain.Enums;

namespace CRM.Application.UserManagement.Services;

public interface IPresenceService
{
    Task UpdatePresenceAsync(Guid userId, PresenceStatus status);
    Task<Dictionary<Guid, PresenceStatus>> GetPresenceStatusesAsync(IEnumerable<Guid> userIds);
    Task NotifyPresenceUpdateAsync(Guid userId, PresenceStatus status);
}

