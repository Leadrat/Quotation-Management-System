using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.UserManagement.Queries;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class ExportUsersQueryHandler
{
    private readonly IAppDbContext _db;

    public ExportUsersQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]> Handle(ExportUsersQuery query)
    {
        // Authorization: Only Admin can export users
        var isAuthorized = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("Only Admin can export users");
        }

        var usersQuery = _db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.DeletedAt == null);

        // Apply filters
        if (query.RoleId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.RoleId == query.RoleId.Value);
        }

        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.CreatedAt <= query.CreatedTo.Value);
        }

        // TODO: Filter by TeamId when team membership is implemented

        var users = await usersQuery
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        // Generate export based on format
        return query.Format.ToUpperInvariant() switch
        {
            "CSV" => GenerateCsv(users),
            "EXCEL" => GenerateExcel(users),
            "JSON" => GenerateJson(users),
            _ => GenerateCsv(users)
        };
    }

    private byte[] GenerateCsv(List<Domain.Entities.User> users)
    {
        var lines = new List<string>
        {
            "Email,FirstName,LastName,Mobile,Role,IsActive,CreatedAt"
        };

        foreach (var user in users)
        {
            var line = $"{user.Email},{user.FirstName},{user.LastName},{user.Mobile ?? ""},{user.Role?.RoleName ?? ""},{user.IsActive},{user.CreatedAt:yyyy-MM-dd HH:mm:ss}";
            lines.Add(line);
        }

        return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines));
    }

    private byte[] GenerateExcel(List<Domain.Entities.User> users)
    {
        // For now, return CSV format (can be enhanced with EPPlus or similar library)
        return GenerateCsv(users);
    }

    private byte[] GenerateJson(List<Domain.Entities.User> users)
    {
        var exportData = users.Select(u => new
        {
            u.UserId,
            u.Email,
            u.FirstName,
            u.LastName,
            u.Mobile,
            Role = u.Role?.RoleName,
            u.IsActive,
            u.CreatedAt
        }).ToList();

        var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        return System.Text.Encoding.UTF8.GetBytes(json);
    }
}

