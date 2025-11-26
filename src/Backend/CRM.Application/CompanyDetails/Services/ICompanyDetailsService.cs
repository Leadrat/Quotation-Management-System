using System.Threading.Tasks;
using CRM.Application.CompanyDetails.Dtos;

namespace CRM.Application.CompanyDetails.Services
{
    public interface ICompanyDetailsService
    {
        Task<CompanyDetailsDto?> GetCompanyDetailsAsync();
        Task<BankDetailsDto?> GetBankDetailsByCountryAsync(string country);
    }
}

