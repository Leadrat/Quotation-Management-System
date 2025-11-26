using System.Text.Json;

namespace CRM.Application.Imports.Services;

public class MappingService
{
    public bool ValidateRequiredMappings(JsonElement mappings, out string? error)
    {
        error = null;
        // Minimal validation: check company.name and customer.name exist when present
        if (!mappings.TryGetProperty("company", out var company) || !company.TryGetProperty("name", out _))
        {
            error = "Missing required mapping: company.name";
            return false;
        }
        if (!mappings.TryGetProperty("customer", out var customer) || !customer.TryGetProperty("name", out _))
        {
            error = "Missing required mapping: customer.name";
            return false;
        }
        return true;
    }
}
