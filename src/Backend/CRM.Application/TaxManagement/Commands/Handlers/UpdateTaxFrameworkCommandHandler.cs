using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.TaxManagement.Commands;
using CRM.Application.TaxManagement.Dtos;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.TaxManagement.Commands.Handlers
{
    public class UpdateTaxFrameworkCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;

        public UpdateTaxFrameworkCommandHandler(IAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<TaxFrameworkDto> Handle(UpdateTaxFrameworkCommand cmd)
        {
            var entity = await _db.TaxFrameworks
                .FirstOrDefaultAsync(f => f.TaxFrameworkId == cmd.TaxFrameworkId && f.DeletedAt == null);
            
            if (entity == null)
            {
                throw new InvalidOperationException($"Tax framework with ID '{cmd.TaxFrameworkId}' not found");
            }

            if (!string.IsNullOrWhiteSpace(cmd.FrameworkName))
                entity.FrameworkName = cmd.FrameworkName.Trim();

            if (cmd.FrameworkType.HasValue)
                entity.FrameworkType = cmd.FrameworkType.Value;

            if (cmd.Description != null)
                entity.Description = string.IsNullOrWhiteSpace(cmd.Description) ? null : cmd.Description.Trim();

            if (cmd.TaxComponents != null && cmd.TaxComponents.Count > 0)
            {
                entity.SetTaxComponents(cmd.TaxComponents);
            }

            if (cmd.IsActive.HasValue)
                entity.IsActive = cmd.IsActive.Value;

            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync();

            // Log configuration change
            var taxLog = new TaxCalculationLog
            {
                LogId = Guid.NewGuid(),
                QuotationId = null,
                ActionType = TaxCalculationActionType.ConfigurationChange,
                CountryId = entity.CountryId,
                JurisdictionId = null,
                CalculationDetails = JsonSerializer.Serialize(new
                {
                    Action = "TaxFrameworkUpdated",
                    TaxFrameworkId = entity.TaxFrameworkId,
                    FrameworkName = entity.FrameworkName,
                    FrameworkType = entity.FrameworkType,
                    TaxComponents = entity.GetTaxComponents()
                }),
                ChangedByUserId = cmd.UpdatedByUserId,
                ChangedAt = DateTimeOffset.UtcNow
            };
            _db.TaxCalculationLogs.Add(taxLog);
            await _db.SaveChangesAsync();

            return _mapper.Map<TaxFrameworkDto>(entity);
        }
    }
}

