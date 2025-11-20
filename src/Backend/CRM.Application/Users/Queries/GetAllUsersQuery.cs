using System;

namespace CRM.Application.Users.Queries
{
    public class GetAllUsersQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public Guid RequestorUserId { get; set; }
        public string RequestorRole { get; set; } = string.Empty; // "Admin" or other roles
    }
}

