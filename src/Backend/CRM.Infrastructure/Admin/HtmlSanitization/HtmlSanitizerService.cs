using Ganss.Xss;

namespace CRM.Infrastructure.Admin.HtmlSanitization;

/// <summary>
/// Implementation of IHtmlSanitizer using Ganss.Xss library
/// </summary>
public class HtmlSanitizerService : IHtmlSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();
        
        // Configure allowed HTML tags
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.UnionWith(new[] { "p", "br", "strong", "em", "a", "ul", "ol", "li" });
        
        // Configure allowed attributes
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.UnionWith(new[] { "href", "style" });
        
        // Configure allowed CSS properties (for style attribute)
        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedCssProperties.UnionWith(new[] { "color", "background-color", "font-size", "font-weight", "text-align" });
        
        // Configure allowed URL schemes
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.UnionWith(new[] { "http", "https", "mailto" });
        
        // Remove dangerous schemes
        _sanitizer.AllowedSchemes.Remove("javascript");
        _sanitizer.AllowedSchemes.Remove("data");
        
        // Remove all event handlers
        _sanitizer.AllowedAttributes.Remove("onclick");
        _sanitizer.AllowedAttributes.Remove("onerror");
        _sanitizer.AllowedAttributes.Remove("onload");
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _sanitizer.Sanitize(html);
    }
}

