using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Application.Quotations.Services
{
    public class QuotationNumberGenerator
    {
        private readonly IAppDbContext _db;
        private readonly QuotationSettings _settings;
        private readonly ILogger<QuotationNumberGenerator>? _logger;

        public QuotationNumberGenerator(IAppDbContext db, IOptions<QuotationSettings> settings, ILogger<QuotationNumberGenerator>? logger = null)
        {
            _db = db;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> GenerateAsync()
        {
            try
            {
                var year = DateTime.UtcNow.Year;
                var format = _settings.NumberFormat;

                // Try to get the next sequence number for this year
                var lastQuotation = await _db.Quotations
                    .Where(q => q.QuotationNumber.StartsWith($"QT-{year}-"))
                    .OrderByDescending(q => q.QuotationNumber)
                    .FirstOrDefaultAsync();

                int sequenceNumber = 1;

                if (lastQuotation != null)
                {
                    // Extract sequence from last quotation number (e.g., QT-2025-001234)
                    var parts = lastQuotation.QuotationNumber.Split('-');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out var lastSequence))
                    {
                        sequenceNumber = lastSequence + 1;
                    }
                }

                // Retry up to 10 times if collision occurs
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    var quotationNumber = format
                        .Replace("{Year}", year.ToString())
                        .Replace("{Sequence}", sequenceNumber.ToString("D6")); // 6-digit padding

                    var exists = await _db.Quotations
                        .AnyAsync(q => q.QuotationNumber == quotationNumber);

                    if (!exists)
                    {
                        return quotationNumber;
                    }

                    sequenceNumber++;
                }

                // Fallback: use GUID if all attempts fail
                return $"QT-{year}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
            }
            catch (Exception ex)
            {
                // Check if this is a missing table error
                var exceptionMessage = ex.Message;
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    exceptionMessage += " | " + innerException.Message;
                    innerException = innerException.InnerException;
                }

                if (exceptionMessage.Contains("42P01") || 
                    exceptionMessage.Contains("does not exist") || 
                    (exceptionMessage.Contains("relation") && exceptionMessage.Contains("not exist")) ||
                    exceptionMessage.Contains("Quotations"))
                {
                    _logger?.LogWarning("Quotations table does not exist, using fallback quotation number generation");
                    // Return a simple fallback number when table doesn't exist
                    var year = DateTime.UtcNow.Year;
                    return $"QT-{year}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
                }

                // For other errors, rethrow
                throw;
            }
        }
    }
}

