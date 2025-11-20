using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Results;
using CRM.Application.QuotationTemplates.Dtos;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.QuotationTemplates.Queries.Handlers
{
    public class GetAllTemplatesQueryHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllTemplatesQueryHandler> _logger;

        public GetAllTemplatesQueryHandler(
            IAppDbContext db,
            IMapper mapper,
            ILogger<GetAllTemplatesQueryHandler> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<QuotationTemplateDto>> Handle(GetAllTemplatesQuery query)
        {
            try
            {
                var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
                var pageSize = query.PageSize > 100 ? 100 : (query.PageSize < 1 ? 10 : query.PageSize);

                var isAdmin = string.Equals(query.RequestorRole, "Admin", StringComparison.OrdinalIgnoreCase);

                // Check if table exists - if not, return empty result
                try
                {
                    // Test if we can query the table
                    await _db.QuotationTemplates.AnyAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning(dbEx, "QuotationTemplates table may not exist or is not accessible");
                    return new PagedResult<QuotationTemplateDto>
                    {
                        Data = Array.Empty<QuotationTemplateDto>(),
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                // Base query - exclude deleted templates
                IQueryable<Domain.Entities.QuotationTemplate> baseQuery = _db.QuotationTemplates
                    .AsNoTracking()
                    .Where(t => t.DeletedAt == null);

                // Apply visibility/authorization filters
                if (!isAdmin)
                {
                    // Non-admin users can see:
                    // 1. Their own templates (any visibility)
                    // 2. Public approved templates
                    // 3. Team templates where owner role matches
                    baseQuery = baseQuery.Where(t =>
                        t.OwnerUserId == query.RequestorUserId ||
                        (t.Visibility == TemplateVisibility.Public && t.IsApproved) ||
                        (t.Visibility == TemplateVisibility.Team && t.OwnerRole == query.RequestorRole));
                }

                // Apply filters
                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    var searchTerm = query.Search.ToLower();
                    baseQuery = baseQuery.Where(t =>
                        t.Name.ToLower().Contains(searchTerm) ||
                        (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(query.Visibility))
                {
                    if (Enum.TryParse<TemplateVisibility>(query.Visibility, true, out var visibility))
                    {
                        baseQuery = baseQuery.Where(t => t.Visibility == visibility);
                    }
                }

                if (query.IsApproved.HasValue)
                {
                    baseQuery = baseQuery.Where(t => t.IsApproved == query.IsApproved.Value);
                }

                if (query.OwnerUserId.HasValue)
                {
                    baseQuery = baseQuery.Where(t => t.OwnerUserId == query.OwnerUserId.Value);
                }

                // Get total count before including navigation properties (more efficient)
                int totalCount;
                try
                {
                    totalCount = await baseQuery.CountAsync();
                }
                catch (Exception countEx)
                {
                    _logger.LogError(countEx, "Error counting templates");
                    totalCount = 0;
                }

                // Include navigation properties only for the paginated results
                // Use defensive includes - if navigation properties fail, we'll handle it in mapping
                List<Domain.Entities.QuotationTemplate> templates;
                try
                {
                    templates = await baseQuery
                        .Include(t => t.OwnerUser)
                        .Include(t => t.ApprovedByUser)
                        .Include(t => t.LineItems)
                        .OrderByDescending(t => t.UpdatedAt)
                        .ThenBy(t => t.Name)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                }
                catch (Exception includeEx)
                {
                    _logger.LogWarning(includeEx, "Error loading templates with navigation properties, trying without includes");
                    // Try without includes if that fails
                    try
                    {
                        templates = await baseQuery
                            .OrderByDescending(t => t.UpdatedAt)
                            .ThenBy(t => t.Name)
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Error loading templates even without includes");
                        return new PagedResult<QuotationTemplateDto>
                        {
                            Data = Array.Empty<QuotationTemplateDto>(),
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            TotalCount = 0
                        };
                    }
                }

                // Map to DTOs with error handling
                var items = templates.Select(t =>
                {
                    try
                    {
                        return _mapper.Map<QuotationTemplateDto>(t);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error mapping template {TemplateId} to DTO: {Error}", t.TemplateId, ex.Message);
                        // Return a basic DTO if mapping fails
                        var dto = new QuotationTemplateDto
                        {
                            TemplateId = t.TemplateId,
                            Name = t.Name ?? "Unknown",
                            Description = t.Description,
                            Visibility = t.Visibility.ToString(),
                            IsApproved = t.IsApproved,
                            OwnerUserId = t.OwnerUserId,
                            OwnerRole = t.OwnerRole ?? "SalesRep",
                            OwnerUserName = t.OwnerUser != null ? $"{t.OwnerUser.FirstName} {t.OwnerUser.LastName}".Trim() : "Unknown",
                            ApprovedByUserId = t.ApprovedByUserId,
                            ApprovedByUserName = t.ApprovedByUser != null ? $"{t.ApprovedByUser.FirstName} {t.ApprovedByUser.LastName}".Trim() : null,
                            ApprovedAt = t.ApprovedAt,
                            Version = t.Version,
                            PreviousVersionId = t.PreviousVersionId,
                            UsageCount = t.UsageCount,
                            LastUsedAt = t.LastUsedAt,
                            CreatedAt = t.CreatedAt,
                            UpdatedAt = t.UpdatedAt,
                            DeletedAt = t.DeletedAt,
                            DiscountDefault = t.DiscountDefault,
                            Notes = t.Notes,
                            LineItems = t.LineItems?.Select(li => new TemplateLineItemDto
                            {
                                LineItemId = li.LineItemId,
                                TemplateId = li.TemplateId,
                                SequenceNumber = li.SequenceNumber,
                                ItemName = li.ItemName,
                                Description = li.Description,
                                Quantity = li.Quantity,
                                UnitRate = li.UnitRate,
                                Amount = li.Amount,
                                CreatedAt = li.CreatedAt
                            }).ToList() ?? new List<TemplateLineItemDto>()
                        };
                        return dto;
                }
            }).ToArray();

                return new PagedResult<QuotationTemplateDto>
                {
                    Data = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllTemplatesQueryHandler: {Error}", ex.Message);
                throw; // Re-throw to be caught by controller
            }
        }
    }
}

