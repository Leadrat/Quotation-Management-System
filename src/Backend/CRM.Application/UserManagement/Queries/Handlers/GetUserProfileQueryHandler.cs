using System;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.UserManagement.Queries;
using CRM.Application.UserManagement.DTOs;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.UserManagement.Queries.Handlers;

public class GetUserProfileQueryHandler
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetUserProfileQueryHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<EnhancedUserProfileDto> Handle(GetUserProfileQuery query)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.DelegateUser)
            .FirstOrDefaultAsync(u => u.UserId == query.UserId);

        if (user == null || !user.IsActive || user.DeletedAt != null)
        {
            throw new InvalidOperationException("User not found or inactive");
        }

        return _mapper.Map<EnhancedUserProfileDto>(user);
    }
}

