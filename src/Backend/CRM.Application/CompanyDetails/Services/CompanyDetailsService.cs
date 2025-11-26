using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.CompanyDetails.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CRM.Application.CompanyDetails.Services
{
    public class CompanyDetailsService : ICompanyDetailsService
    {
        private readonly IAppDbContext _db;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "CompanyDetails";

        public CompanyDetailsService(IAppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<CompanyDetailsDto?> GetCompanyDetailsAsync()
        {
            // Try cache first
            if (_cache.TryGetValue(CacheKey, out CompanyDetailsDto? cached))
            {
                return cached;
            }

            var companyDetails = await _db.CompanyDetails
                .Include(c => c.BankDetails)
                .FirstOrDefaultAsync();

            if (companyDetails == null)
            {
                return null;
            }

            var dto = new CompanyDetailsDto
            {
                CompanyDetailsId = companyDetails.CompanyDetailsId,
                PanNumber = companyDetails.PanNumber,
                TanNumber = companyDetails.TanNumber,
                GstNumber = companyDetails.GstNumber,
                CompanyName = companyDetails.CompanyName,
                CompanyAddress = companyDetails.CompanyAddress,
                City = companyDetails.City,
                State = companyDetails.State,
                PostalCode = companyDetails.PostalCode,
                Country = companyDetails.Country,
                ContactEmail = companyDetails.ContactEmail,
                ContactPhone = companyDetails.ContactPhone,
                Website = companyDetails.Website,
                LegalDisclaimer = companyDetails.LegalDisclaimer,
                LogoUrl = companyDetails.LogoUrl,
                UpdatedAt = companyDetails.UpdatedAt,
                BankDetails = companyDetails.BankDetails.Select(b => new BankDetailsDto
                {
                    BankDetailsId = b.BankDetailsId,
                    Country = b.Country,
                    AccountNumber = b.AccountNumber,
                    IfscCode = b.IfscCode,
                    Iban = b.Iban,
                    SwiftCode = b.SwiftCode,
                    BankName = b.BankName,
                    BranchName = b.BranchName
                }).ToList()
            };

            // Cache for 5 minutes
            _cache.Set(CacheKey, dto, TimeSpan.FromMinutes(5));

            return dto;
        }

        public async Task<BankDetailsDto?> GetBankDetailsByCountryAsync(string country)
        {
            var companyDetails = await GetCompanyDetailsAsync();
            if (companyDetails == null)
            {
                return null;
            }

            return companyDetails.BankDetails.FirstOrDefault(b => b.Country == country);
        }
    }
}

