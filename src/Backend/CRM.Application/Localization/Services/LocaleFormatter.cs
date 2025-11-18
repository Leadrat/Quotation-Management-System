using System;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Localization.Services;

public class LocaleFormatter : ILocaleFormatter
{
    private readonly ILogger<LocaleFormatter> _logger;

    public LocaleFormatter(ILogger<LocaleFormatter> logger)
    {
        _logger = logger;
    }

    public string FormatCurrency(decimal amount, string currencyCode, string locale)
    {
        try
        {
            var culture = new CultureInfo(locale);
            return amount.ToString("C", culture);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format currency with locale {Locale}, using default", locale);
            return $"{currencyCode} {amount:N2}";
        }
    }

    public string FormatDate(DateTime date, string locale, string? format = null)
    {
        try
        {
            var culture = new CultureInfo(locale);
            if (!string.IsNullOrEmpty(format))
            {
                return date.ToString(format, culture);
            }
            return date.ToString("d", culture);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format date with locale {Locale}, using default", locale);
            return date.ToString("dd/MM/yyyy");
        }
    }

    public string FormatNumber(decimal number, string locale)
    {
        try
        {
            var culture = new CultureInfo(locale);
            return number.ToString("N", culture);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format number with locale {Locale}, using default", locale);
            return number.ToString("N2");
        }
    }

    public string FormatDateTime(DateTimeOffset dateTime, string locale)
    {
        try
        {
            var culture = new CultureInfo(locale);
            return dateTime.ToString("g", culture);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format datetime with locale {Locale}, using default", locale);
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }
    }

    public DateTime? ParseDate(string dateString, string locale, string format)
    {
        try
        {
            var culture = new CultureInfo(locale);
            if (DateTime.TryParseExact(dateString, format, culture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse date with locale {Locale} and format {Format}", locale, format);
            return null;
        }
    }

    public decimal? ParseNumber(string numberString, string locale)
    {
        try
        {
            var culture = new CultureInfo(locale);
            if (decimal.TryParse(numberString, NumberStyles.Number, culture, out var result))
            {
                return result;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse number with locale {Locale}", locale);
            return null;
        }
    }
}


