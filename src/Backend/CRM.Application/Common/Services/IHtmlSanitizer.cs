namespace CRM.Application.Common.Services;

public interface IHtmlSanitizer
{
    string Sanitize(string html);
}

