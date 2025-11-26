using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class ExportClientsQueryHandler
    {
        private readonly IAppDbContext _db;
        public ExportClientsQueryHandler(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Client>> Handle(ExportClientsQuery q)
        {
            var isAdmin = string.Equals(q.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            var isManager = string.Equals(q.RequestorRole, "Manager", StringComparison.OrdinalIgnoreCase);
            var max = q.MaxRows <= 0 ? 10000 : (q.MaxRows > 10000 ? 10000 : q.MaxRows);

            var query = _db.Clients.AsNoTracking().Where(c => c.DeletedAt == null);

            // Admin and Manager see all clients
            if (!isAdmin && !isManager)
            {
                query = query.Where(c => c.CreatedByUserId == q.RequestorUserId);
            }
            else if (q.CreatedByUserId.HasValue)
            {
                query = query.Where(c => c.CreatedByUserId == q.CreatedByUserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q.City))
            {
                var city = q.City.Trim().ToLower();
                query = query.Where(c => c.City != null && c.City.ToLower().Contains(city));
            }
            if (!string.IsNullOrWhiteSpace(q.State))
            {
                var state = q.State.Trim().ToLower();
                query = query.Where(c => c.State != null && c.State.ToLower().Contains(state));
            }
            if (!string.IsNullOrWhiteSpace(q.StateCode))
            {
                query = query.Where(c => c.StateCode == q.StateCode);
            }
            if (!string.IsNullOrWhiteSpace(q.Gstin))
            {
                var gst = q.Gstin.Trim();
                query = query.Where(c => c.Gstin != null && c.Gstin.Contains(gst));
            }
            if (q.CreatedDateFrom.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= q.CreatedDateFrom.Value);
            }
            if (q.CreatedDateTo.HasValue)
            {
                query = query.Where(c => c.CreatedAt < q.CreatedDateTo.Value);
            }
            if (q.UpdatedDateFrom.HasValue)
            {
                query = query.Where(c => c.UpdatedAt >= q.UpdatedDateFrom.Value);
            }
            if (q.UpdatedDateTo.HasValue)
            {
                query = query.Where(c => c.UpdatedAt < q.UpdatedDateTo.Value);
            }
            if (!string.IsNullOrWhiteSpace(q.SearchTerm))
            {
                var lower = q.SearchTerm.Trim().ToLower();
                query = query.Where(c =>
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(lower)) ||
                    (c.ContactName != null && c.ContactName.ToLower().Contains(lower)) ||
                    (c.Email != null && c.Email.ToLower().Contains(lower))
                );
            }

            query = q.SortBy switch
            {
                "NameAsc" => query.OrderBy(c => c.CompanyName).ThenByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId),
                "NameDesc" => query.OrderByDescending(c => c.CompanyName).ThenByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId),
                "CreatedAtAsc" => query.OrderBy(c => c.CreatedAt).ThenBy(c => c.ClientId),
                "CreatedAtDesc" => query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId),
                "UpdatedAtDesc" => query.OrderByDescending(c => c.UpdatedAt).ThenBy(c => c.ClientId),
                "EmailAsc" => query.OrderBy(c => c.Email).ThenByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId),
                _ => query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.ClientId)
            };

            return await query.Take(max).ToListAsync();
        }
    }
}
