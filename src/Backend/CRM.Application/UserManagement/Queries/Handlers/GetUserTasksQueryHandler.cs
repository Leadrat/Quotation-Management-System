using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class GetUserTasksQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetUserTasksQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<TaskAssignmentDto>> Handle(GetUserTasksQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize > 100 ? 100 : (query.PageSize < 1 ? 10 : query.PageSize);

        // Authorization: Users can only view their own tasks unless they're Admin/Manager
        var isAuthorized = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(query.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase) ||
                          query.RequestorUserId == query.UserId;
        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("Not authorized to view this user's tasks");
        }

        var tasksQuery = _db.TaskAssignments
            .AsNoTracking()
            .Include(ta => ta.AssignedToUser)
            .Include(ta => ta.AssignedByUser)
            .Where(ta => ta.AssignedToUserId == query.UserId);

        // Filter by status
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var statusEnum = query.Status switch
            {
                "Pending" => TaskAssignmentStatus.Pending,
                "InProgress" => TaskAssignmentStatus.InProgress,
                "Completed" => TaskAssignmentStatus.Completed,
                "Cancelled" => TaskAssignmentStatus.Cancelled,
                _ => (TaskAssignmentStatus?)null
            };
            if (statusEnum.HasValue)
            {
                tasksQuery = tasksQuery.Where(ta => ta.Status == statusEnum.Value);
            }
        }

        // Filter by entity type
        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            tasksQuery = tasksQuery.Where(ta => ta.EntityType == query.EntityType);
        }

        // Filter by due date range
        if (query.DueDateFrom.HasValue)
        {
            tasksQuery = tasksQuery.Where(ta => ta.DueDate.HasValue && ta.DueDate.Value >= query.DueDateFrom.Value);
        }
        if (query.DueDateTo.HasValue)
        {
            tasksQuery = tasksQuery.Where(ta => ta.DueDate.HasValue && ta.DueDate.Value <= query.DueDateTo.Value);
        }

        var total = await tasksQuery.CountAsync();

        var tasks = await tasksQuery
            .OrderByDescending(ta => ta.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var dtos = tasks.Select(t => new TaskAssignmentDto
        {
            AssignmentId = t.AssignmentId,
            EntityType = t.EntityType,
            EntityId = t.EntityId,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUserName = t.AssignedToUser?.GetFullName() ?? string.Empty,
            AssignedByUserId = t.AssignedByUserId,
            AssignedByUserName = t.AssignedByUser?.GetFullName() ?? string.Empty,
            DueDate = t.DueDate,
            Status = t.Status.ToString(),
            IsOverdue = t.IsOverdue(),
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToArray();

        return new PagedResult<TaskAssignmentDto>
        {
            Success = true,
            Data = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}

