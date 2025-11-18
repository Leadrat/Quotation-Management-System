using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Clients.Commands;
using CRM.Application.Clients.Dtos;
using CRM.Application.Clients.Exceptions;
using CRM.Application.Clients.Services;
using CRM.Application.Common.Persistence;
using CRM.Domain.Entities;
using CRM.Domain.Events;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRM.Application.Clients.Commands.Handlers
{
    public class RestoreClientCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly HistorySettings _historySettings;
        private readonly ClientHistoryDiffBuilder _diffBuilder;

        public RestoreClientCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            IOptions<HistorySettings> historySettings,
            ClientHistoryDiffBuilder diffBuilder)
        {
            _db = db;
            _mapper = mapper;
            _historySettings = historySettings.Value;
            _diffBuilder = diffBuilder;
        }

        public async Task<ClientDto> Handle(RestoreClientCommand command)
        {
            var entity = await _db.Clients
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.ClientId == command.ClientId);

            if (entity == null)
            {
                throw new ClientNotFoundException(command.ClientId);
            }

            var isAdmin = string.Equals(command.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("Only admins can restore clients.");
            }

            if (!entity.DeletedAt.HasValue)
            {
                throw new InvalidOperationException("Client is already active.");
            }

            var now = DateTimeOffset.UtcNow;
            var expiration = entity.DeletedAt.Value.AddDays(_historySettings.RestoreWindowDays);
            if (now > expiration)
            {
                throw new InvalidOperationException("Restoration window expired.");
            }

            var before = new Dictionary<string, object?>
            {
                { "DeletedAt", entity.DeletedAt }
            };

            var after = new Dictionary<string, object?>
            {
                { "DeletedAt", null }
            };

            var diff = _diffBuilder.Build(before, after);

            entity.DeletedAt = null;
            entity.UpdatedAt = now;

            var historyEntry = new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = entity.ClientId,
                ActorUserId = command.RequestorUserId,
                ActionType = "RESTORED",
                ChangedFields = new List<string>(diff.ChangedFields),
                BeforeSnapshot = diff.BeforeSnapshotJson,
                AfterSnapshot = diff.AfterSnapshotJson,
                Reason = command.Reason,
                Metadata = string.IsNullOrWhiteSpace(command.MetadataJson) ? "{}" : command.MetadataJson,
                CreatedAt = now
            };

            _db.ClientHistories.Add(historyEntry);

            var restoredEvent = new ClientRestored
            {
                ClientId = entity.ClientId,
                CompanyName = entity.CompanyName,
                RestoredByUserId = command.RequestorUserId,
                RestoredAt = now,
                Reason = command.Reason
            };
            _ = restoredEvent; // placeholder for future event publishing

            await _db.SaveChangesAsync();
            return _mapper.Map<ClientDto>(entity);
        }
    }
}

