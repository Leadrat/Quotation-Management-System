using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyDetails.Commands;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CRM.Application.CompanyDetails.Commands.Handlers
{
    public class UpdateCompanyDetailsCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "CompanyDetails";

        public UpdateCompanyDetailsCommandHandler(
            IAppDbContext db,
            IMapper mapper,
            IMemoryCache cache)
        {
            _db = db;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<CompanyDetailsDto> Handle(UpdateCompanyDetailsCommand command)
        {
            // Get or create company details (singleton)
            var companyDetails = await _db.CompanyDetails
                .Include(c => c.BankDetails)
                .FirstOrDefaultAsync();

            var now = DateTimeOffset.UtcNow;

            if (companyDetails == null)
            {
                // Create new record
                companyDetails = new Domain.Entities.CompanyDetails
                {
                    CompanyDetailsId = new Guid("00000000-0000-0000-0000-000000000001"),
                    CreatedAt = now
                };
                _db.CompanyDetails.Add(companyDetails);
            }

            // Update properties (legacy fields - keep for backward compatibility)
            companyDetails.PanNumber = command.Request.PanNumber;
            companyDetails.TanNumber = command.Request.TanNumber;
            companyDetails.GstNumber = command.Request.GstNumber;
            companyDetails.CompanyName = command.Request.CompanyName;
            companyDetails.CompanyAddress = command.Request.CompanyAddress;
            companyDetails.City = command.Request.City;
            companyDetails.State = command.Request.State;
            companyDetails.PostalCode = command.Request.PostalCode;
            companyDetails.Country = command.Request.Country;
            companyDetails.CountryId = command.Request.CountryId; // Update CountryId
            companyDetails.ContactEmail = command.Request.ContactEmail;
            companyDetails.ContactPhone = command.Request.ContactPhone;
            companyDetails.Website = command.Request.Website;
            companyDetails.LegalDisclaimer = command.Request.LegalDisclaimer;
            companyDetails.LogoUrl = command.Request.LogoUrl;
            companyDetails.UpdatedAt = now;
            companyDetails.UpdatedBy = command.UpdatedBy;

            // Store country-specific basic details in JSONB if CountryId is provided
            if (command.Request.CountryId.HasValue)
            {
                var countryIdStr = command.Request.CountryId.Value.ToString();
                
                // Deserialize existing country details or create new dictionary
                var countryDetails = new Dictionary<string, Dictionary<string, string>>();
                if (!string.IsNullOrWhiteSpace(companyDetails.CountryDetails))
                {
                    try
                    {
                        countryDetails = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                            companyDetails.CountryDetails) ?? new Dictionary<string, Dictionary<string, string>>();
                    }
                    catch
                    {
                        // If deserialization fails, start with empty dictionary
                    }
                }
                
                // Store country-specific details
                if (!countryDetails.ContainsKey(countryIdStr))
                {
                    countryDetails[countryIdStr] = new Dictionary<string, string>();
                }
                
                countryDetails[countryIdStr]["CompanyName"] = command.Request.CompanyName ?? "";
                countryDetails[countryIdStr]["CompanyAddress"] = command.Request.CompanyAddress ?? "";
                countryDetails[countryIdStr]["City"] = command.Request.City ?? "";
                countryDetails[countryIdStr]["State"] = command.Request.State ?? "";
                countryDetails[countryIdStr]["PostalCode"] = command.Request.PostalCode ?? "";
                countryDetails[countryIdStr]["Country"] = command.Request.Country ?? "";
                countryDetails[countryIdStr]["ContactEmail"] = command.Request.ContactEmail ?? "";
                countryDetails[countryIdStr]["ContactPhone"] = command.Request.ContactPhone ?? "";
                countryDetails[countryIdStr]["Website"] = command.Request.Website ?? "";
                countryDetails[countryIdStr]["LogoUrl"] = command.Request.LogoUrl ?? "";
                countryDetails[countryIdStr]["LegalDisclaimer"] = command.Request.LegalDisclaimer ?? "";
                
                // Serialize and store
                companyDetails.CountryDetails = JsonSerializer.Serialize(countryDetails);
            }

            // Update bank details
            var existingBankDetails = companyDetails.BankDetails.ToList();

            foreach (var bankDetailDto in command.Request.BankDetails)
            {
                var existing = existingBankDetails
                    .FirstOrDefault(b => b.Country == bankDetailDto.Country);

                if (existing != null)
                {
                    // Update existing
                    existing.AccountNumber = bankDetailDto.AccountNumber;
                    existing.IfscCode = bankDetailDto.IfscCode;
                    existing.Iban = bankDetailDto.Iban;
                    existing.SwiftCode = bankDetailDto.SwiftCode;
                    existing.BankName = bankDetailDto.BankName;
                    existing.BranchName = bankDetailDto.BranchName;
                    existing.UpdatedAt = now;
                    existing.UpdatedBy = command.UpdatedBy;
                }
                else
                {
                    // Create new
                    var newBankDetail = new Domain.Entities.BankDetails
                    {
                        BankDetailsId = bankDetailDto.BankDetailsId == Guid.Empty ? Guid.NewGuid() : bankDetailDto.BankDetailsId,
                        CompanyDetailsId = companyDetails.CompanyDetailsId,
                        Country = bankDetailDto.Country,
                        AccountNumber = bankDetailDto.AccountNumber,
                        IfscCode = bankDetailDto.IfscCode,
                        Iban = bankDetailDto.Iban,
                        SwiftCode = bankDetailDto.SwiftCode,
                        BankName = bankDetailDto.BankName,
                        BranchName = bankDetailDto.BranchName,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = command.UpdatedBy
                    };
                    _db.BankDetails.Add(newBankDetail);
                }
            }

            // Remove bank details not in request
            var countriesToKeep = command.Request.BankDetails.Select(b => b.Country).ToList();
            var toRemove = existingBankDetails
                .Where(b => !countriesToKeep.Contains(b.Country))
                .ToList();
            _db.BankDetails.RemoveRange(toRemove);

            await _db.SaveChangesAsync();

            // Invalidate cache
            _cache.Remove(CacheKey);

            // Return updated DTO
            return await GetCompanyDetailsDto(companyDetails.CompanyDetailsId);
        }

        private async Task<CompanyDetailsDto> GetCompanyDetailsDto(Guid companyDetailsId)
        {
            var companyDetails = await _db.CompanyDetails
                .Include(c => c.BankDetails)
                .FirstOrDefaultAsync(c => c.CompanyDetailsId == companyDetailsId);

            return _mapper.Map<CompanyDetailsDto>(companyDetails);
        }
    }
}

