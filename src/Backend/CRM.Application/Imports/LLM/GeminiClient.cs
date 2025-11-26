using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CRM.Application.Imports.LLM;

public class GeminiClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    public GeminiClient(HttpClient httpClient)
    {
        _http = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
        _model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.5-flash";
        _baseUrl = Environment.GetEnvironmentVariable("GEMINI_API_BASE") ?? "https://generativelanguage.googleapis.com";
    }

    public async Task<string> ChatAsync(string prompt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("GEMINI_API_KEY is not configured");

        var url = $"{_baseUrl}/v1beta/models/{_model}:generateContent?key={_apiKey}";

        // Minimal payload for text prompts
        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            })
        };
        using var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        // Extract first text candidate
        var text = root.GetProperty("candidates")[0]
                       .GetProperty("content")
                       .GetProperty("parts")[0]
                       .GetProperty("text").GetString();
        return text ?? string.Empty;
    }
}
