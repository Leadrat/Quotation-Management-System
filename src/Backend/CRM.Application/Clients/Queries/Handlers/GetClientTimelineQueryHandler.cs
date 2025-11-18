using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Exceptions;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRM.Application.Clients.Queries.Handlers
{
    public class GetClientTimelineQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly HistorySettings _historySettings;

        public GetClientTimelineQueryHandler(IAppDbContext db, IMapper mapper, IOptions<HistorySettings> historySettings)
        {
            _db = db;
            _mapper = mapper;
            _historySettings = historySettings.Value;
        }

        public async Task<ClientTimelineSummaryDto> Handle(GetClientTimelineQuery request)
        {
            var client = await _db.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == request.ClientId);

            if (client == null)
            {
                throw new ClientNotFoundException(request.ClientId);
            }

            var isAdmin = string.Equals(request.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && client.CreatedByUserId != request.RequestorUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this client timeline.");
            }

            var summary = _mapper.Map<ClientTimelineSummaryDto>(client);

            var historyQuery = _db.ClientHistories.AsNoTracking()
                .Where(h => h.ClientId == request.ClientId);

            summary.TotalChangeCount = await historyQuery.LongCountAsync();

            var latest = await historyQuery
                .OrderByDescending(h => h.CreatedAt)
                .Include(h => h.ActorUser)
                .FirstOrDefaultAsync();

            if (latest != null)
            {
                summary.LastModifiedAt = latest.CreatedAt;
                summary.LastModifiedBy = ResolveActorName(latest);
                summary.LatestEntry = _mapper.Map<ClientHistoryEntryDto>(latest);
            }

            if (client.DeletedAt.HasValue)
            {
                summary.RestorationWindowExpiresAt = client.DeletedAt.Value.AddDays(_historySettings.RestoreWindowDays);

                var deletedEntry = await historyQuery
                    .Where(h => h.ActionType == "DELETED")
                    .OrderByDescending(h => h.CreatedAt)
                    .Include(h => h.ActorUser)
                    .FirstOrDefaultAsync();

                if (deletedEntry != null)
                {
                    summary.DeletedAt = deletedEntry.CreatedAt;
                    summary.DeletedBy = ResolveActorName(deletedEntry);
                    summary.DeletionReason = deletedEntry.Reason;
                }
            }

            return summary;
        }

        private static string? ResolveActorName(ClientHistory history)
        {
            if (history.ActorUser == null) return "System";
            var full = $"{history.ActorUser.FirstName} {history.ActorUser.LastName}".Trim();
            return string.IsNullOrWhiteSpace(full) ? history.ActorUser.Email : full;
        }
    }
}

