using System;

namespace CRM.Application.Localization.Services;

public interface ILocaleFormatter
{
    string FormatCurrency(decimal amount, string currencyCode, string locale);
    string FormatDate(DateTime date, string locale, string? format = null);
    string FormatNumber(decimal number, string locale);
    string FormatDateTime(DateTimeOffset dateTime, string locale);
    DateTime? ParseDate(string dateString, string locale, string format);
    decimal? ParseNumber(string numberString, string locale);
}


