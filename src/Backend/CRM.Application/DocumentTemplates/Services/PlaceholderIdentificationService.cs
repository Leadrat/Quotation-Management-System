using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CRM.Application.DocumentTemplates.Services
{
    public class PlaceholderIdentificationService : IPlaceholderIdentificationService
    {
        private static readonly string[] CustomerSectionKeywords =
        {
            "bill to", "ship to", "customer:", "client:", "client details", "recipient", "buyer", "client name"
        };

        private static readonly string[] CompanySectionKeywords =
        {
            "seller", "from:", "company details", "our company", "vendor", "supplier"
        };

        private static readonly string[] CountryHints =
        {
            "India", "United States", "USA", "United Kingdom", "UK", "United Arab Emirates", "UAE",
            "Canada", "Australia", "Singapore", "Germany", "France", "Spain", "Italy", "Netherlands",
            "Japan", "China", "Hong Kong"
        };

        public async Task<List<IdentifiedPlaceholder>> IdentifyCompanyDetailsAsync(string documentText, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var (companySection, _) = SplitDocumentSections(documentText);
                var searchText = string.IsNullOrWhiteSpace(companySection) ? documentText : companySection;
                var placeholders = new List<IdentifiedPlaceholder>();

                AddCompanyNamePlaceholders(searchText, placeholders);
                AddAddressPlaceholders(searchText, "Company", placeholders);
                AddCityStatePostalPlaceholders(searchText, "Company", placeholders);
                AddCountryPlaceholder(searchText, "CompanyCountry", "Company", placeholders);
                AddBankPlaceholders(documentText, placeholders);
                AddTaxPlaceholders(documentText, placeholders);

                return Deduplicate(placeholders);
            }, cancellationToken);
        }

        public async Task<List<IdentifiedPlaceholder>> IdentifyCustomerDetailsAsync(string documentText, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var (_, customerSection) = SplitDocumentSections(documentText);
                var searchText = string.IsNullOrWhiteSpace(customerSection) ? documentText : customerSection;
                var placeholders = new List<IdentifiedPlaceholder>();

                AddCustomerNamePlaceholders(searchText, placeholders);
                AddAddressPlaceholders(searchText, "Customer", placeholders);
                AddCityStatePostalPlaceholders(searchText, "Customer", placeholders);
                AddCountryPlaceholder(searchText, "CustomerCountry", "Customer", placeholders);
                AddCustomerGstPlaceholder(searchText, placeholders);

                return Deduplicate(placeholders);
            }, cancellationToken);
        }

        private static (string companySection, string customerSection) SplitDocumentSections(string documentText)
        {
            if (string.IsNullOrWhiteSpace(documentText))
            {
                return (string.Empty, string.Empty);
            }

            var lower = documentText.ToLowerInvariant();
            var customerIndex = FindFirstIndex(lower, CustomerSectionKeywords);

            if (customerIndex >= 0)
            {
                var companySection = documentText[..customerIndex];
                var customerSection = documentText[customerIndex..];
                return (companySection, customerSection);
            }

            var companyIndex = FindFirstIndex(lower, CompanySectionKeywords);
            if (companyIndex >= 0 && companyIndex + 1 < documentText.Length)
            {
                var companySection = documentText[companyIndex..];
                return (companySection, string.Empty);
            }

            return (documentText, string.Empty);
        }

        private static int FindFirstIndex(string source, IEnumerable<string> keywords)
        {
            var index = -1;
            foreach (var keyword in keywords)
            {
                var position = source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (position >= 0 && (index == -1 || position < index))
                {
                    index = position;
                }
            }

            return index;
        }

        private static void AddCompanyNamePlaceholders(string text, List<IdentifiedPlaceholder> placeholders)
        {
            var companyNamePatterns = new[]
            {
                @"(?:Company|Business|Organization|Corp|Inc|Ltd|LLC|Pvt|Limited)[\s:]+([A-Z][A-Za-z0-9\s&.,-]+)",
                @"(?:Name|Company Name)[\s:]+([A-Z][A-Za-z0-9\s&.,-]+)"
            };

            foreach (var pattern in companyNamePatterns)
            {
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1 && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    {
                        placeholders.Add(new IdentifiedPlaceholder
                        {
                            PlaceholderName = "CompanyName",
                            PlaceholderType = "Company",
                            OriginalText = match.Groups[1].Value.Trim(),
                            PositionInDocument = match.Index
                        });
                    }
                }
            }
        }

        private static void AddCustomerNamePlaceholders(string text, List<IdentifiedPlaceholder> placeholders)
        {
            var customerPatterns = new[]
            {
                @"(?:Bill\s+To|Ship\s+To|Customer|Client|Recipient)[\s:]+([A-Z][A-Za-z0-9\s&.,-]+)",
                @"(?:Client Name|Customer Name)[\s:]+([A-Z][A-Za-z0-9\s&.,-]+)"
            };

            foreach (var pattern in customerPatterns)
            {
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1 && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    {
                        placeholders.Add(new IdentifiedPlaceholder
                        {
                            PlaceholderName = "CustomerCompanyName",
                            PlaceholderType = "Customer",
                            OriginalText = match.Groups[1].Value.Trim(),
                            PositionInDocument = match.Index
                        });
                    }
                }
            }
        }

        private static void AddAddressPlaceholders(string text, string placeholderPrefix, List<IdentifiedPlaceholder> placeholders)
        {
            var addressPattern = @"(\d+\s+[A-Za-z0-9\s,.-]+(?:Street|St|Avenue|Ave|Road|Rd|Lane|Ln|Drive|Dr|Boulevard|Blvd|Circle|Cir)[\s,]*[A-Za-z\s,.-]*)";
            var addressMatches = Regex.Matches(text, addressPattern, RegexOptions.IgnoreCase);
            foreach (Match match in addressMatches)
            {
                placeholders.Add(new IdentifiedPlaceholder
                {
                    PlaceholderName = $"{placeholderPrefix}Address",
                    PlaceholderType = placeholderPrefix,
                    OriginalText = match.Value.Trim(),
                    PositionInDocument = match.Index
                });
            }
        }

        private static void AddCityStatePostalPlaceholders(string text, string placeholderPrefix, List<IdentifiedPlaceholder> placeholders)
        {
            var cityStateZipPattern = @"([A-Z][a-z]+(?:\s+[A-Z][a-z]+)*)[\s,]+([A-Z]{2})[\s,]+(\d{4,10})";
            var cityMatches = Regex.Matches(text, cityStateZipPattern);
            foreach (Match match in cityMatches)
            {
                if (match.Groups.Count >= 4)
                {
                    placeholders.Add(new IdentifiedPlaceholder
                    {
                        PlaceholderName = $"{placeholderPrefix}City",
                        PlaceholderType = placeholderPrefix,
                        OriginalText = match.Groups[1].Value.Trim(),
                        PositionInDocument = match.Index
                    });
                    placeholders.Add(new IdentifiedPlaceholder
                    {
                        PlaceholderName = $"{placeholderPrefix}State",
                        PlaceholderType = placeholderPrefix,
                        OriginalText = match.Groups[2].Value.Trim(),
                        PositionInDocument = match.Index + match.Groups[2].Index
                    });
                    placeholders.Add(new IdentifiedPlaceholder
                    {
                        PlaceholderName = $"{placeholderPrefix}PostalCode",
                        PlaceholderType = placeholderPrefix,
                        OriginalText = match.Groups[3].Value.Trim(),
                        PositionInDocument = match.Index + match.Groups[3].Index
                    });
                }
            }
        }

        private static void AddCountryPlaceholder(string text, string placeholderName, string placeholderType, List<IdentifiedPlaceholder> placeholders)
        {
            foreach (var country in CountryHints)
            {
                var match = Regex.Match(text, $@"\b{Regex.Escape(country)}\b", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    placeholders.Add(new IdentifiedPlaceholder
                    {
                        PlaceholderName = placeholderName,
                        PlaceholderType = placeholderType,
                        OriginalText = match.Value.Trim(),
                        PositionInDocument = match.Index
                    });
                    break;
                }
            }
        }

        private static void AddBankPlaceholders(string documentText, List<IdentifiedPlaceholder> placeholders)
        {
            var bankAccountPatterns = new[]
            {
                new { Pattern = @"(?:Account|A/C|Acc)[\s:]+(\d{8,20})", Name = "BankAccountNumber" },
                new { Pattern = @"(?:IFSC|IFSC Code)[\s:]+([A-Z]{4}0[A-Z0-9]{6})", Name = "BankIFSC" },
                new { Pattern = @"(?:IBAN)[\s:]+([A-Z]{2}\d{2}[A-Z0-9]{4,30})", Name = "BankIBAN" },
                new { Pattern = @"(?:SWIFT|BIC)[\s:]+([A-Z]{4}[A-Z]{2}[A-Z0-9]{2}(?:[A-Z0-9]{3})?)", Name = "BankSWIFT" },
                new { Pattern = @"(?:Bank|Bank Name)[\s:]+([A-Z][A-Za-z\s]+)", Name = "BankName" },
                new { Pattern = @"(?:Branch|Branch Name)[\s:]+([A-Z][A-Za-z\s]+)", Name = "BankBranch" }
            };

            foreach (var bankPattern in bankAccountPatterns)
            {
                var matches = Regex.Matches(documentText, bankPattern.Pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1 && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    {
                        placeholders.Add(new IdentifiedPlaceholder
                        {
                            PlaceholderName = bankPattern.Name,
                            PlaceholderType = "Company",
                            OriginalText = match.Groups[1].Value.Trim(),
                            PositionInDocument = match.Index
                        });
                    }
                }
            }
        }

        private static void AddTaxPlaceholders(string documentText, List<IdentifiedPlaceholder> placeholders)
        {
            var taxPatterns = new[]
            {
                new { Pattern = @"(?:PAN|Permanent Account Number)[\s:]+([A-Z]{5}\d{4}[A-Z])", Name = "CompanyPAN" },
                new { Pattern = @"(?:TAN|Tax Deduction Account Number)[\s:]+([A-Z]{4}\d{5}[A-Z])", Name = "CompanyTAN" },
                new { Pattern = @"(?:GST|GSTIN|GST Number)[\s:]+([0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1})", Name = "CompanyGST" }
            };

            foreach (var taxPattern in taxPatterns)
            {
                var matches = Regex.Matches(documentText, taxPattern.Pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1 && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    {
                        placeholders.Add(new IdentifiedPlaceholder
                        {
                            PlaceholderName = taxPattern.Name,
                            PlaceholderType = "Company",
                            OriginalText = match.Groups[1].Value.Trim(),
                            PositionInDocument = match.Index
                        });
                    }
                }
            }
        }

        private static void AddCustomerGstPlaceholder(string documentText, List<IdentifiedPlaceholder> placeholders)
        {
            var customerGstPattern = @"(?:Customer|Client|Bill\s+To|Ship\s+To)[\s:]+[A-Za-z0-9\s&.,-]+[\s,]+[A-Za-z0-9\s,.-]+[\s,]+(?:GST|GSTIN|GST Number)[\s:]+([0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1})";
            var gstMatches = Regex.Matches(documentText, customerGstPattern, RegexOptions.IgnoreCase);
            foreach (Match match in gstMatches)
            {
                if (match.Groups.Count > 1)
                {
                    placeholders.Add(new IdentifiedPlaceholder
                    {
                        PlaceholderName = "CustomerGSTIN",
                        PlaceholderType = "Customer",
                        OriginalText = match.Groups[1].Value.Trim(),
                        PositionInDocument = match.Index
                    });
                }
            }
        }

        private static List<IdentifiedPlaceholder> Deduplicate(IEnumerable<IdentifiedPlaceholder> placeholders)
        {
            return placeholders
                .Where(p => !string.IsNullOrWhiteSpace(p.OriginalText))
                .GroupBy(p => $"{p.PlaceholderName}:{p.OriginalText}".ToLowerInvariant())
                .Select(g => g.OrderBy(p => p.PositionInDocument ?? int.MaxValue).First())
                .ToList();
        }
    }
}
