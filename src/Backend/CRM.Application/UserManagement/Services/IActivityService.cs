using System;
using System.Threading.Tasks;
using CRM.Application.UserManagement.DTOs;
using CRM.Domain.UserManagement;

namespace CRM.Application.UserManagement.Services;

public interface IActivityService
{
    Task LogActivityAsync(Guid userId, string actionType, string? entityType, Guid? entityId, string ipAddress, string userAgent);
    Task<UserActivity> CreateActivityAsync(Guid userId, string actionType, string? entityType, Guid? entityId, string ipAddress, string userAgent);
}

