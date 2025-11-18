namespace CRM.Infrastructure.Admin.HtmlSanitization;

/// <summary>
/// Service for sanitizing HTML content to prevent XSS attacks
/// </summary>
public interface IHtmlSanitizer
{
    /// <summary>
    /// Sanitizes HTML content by removing dangerous elements and attributes
    /// </summary>
    /// <param name="html">HTML content to sanitize</param>
    /// <returns>Sanitized HTML content</returns>
    string Sanitize(string html);
}

