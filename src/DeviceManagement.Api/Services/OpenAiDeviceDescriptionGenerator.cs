using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DeviceManagement.Api.Models;
using DeviceManagement.Api.Options;
using Microsoft.Extensions.Options;

namespace DeviceManagement.Api.Services;

public class OpenAiDeviceDescriptionGenerator : IDeviceDescriptionGenerator
{
    private readonly HttpClient _http;
    private readonly OpenAiOptions _options;

    public OpenAiDeviceDescriptionGenerator(HttpClient http, IOptions<OpenAiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<string> GenerateAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("OpenAI:ApiKey is not configured. Use user-secrets or environment variables.");

        using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var typeLabel = device.Type == DeviceType.Phone ? "phone" : "tablet";
        var system =
            "You write short, professional inventory descriptions for IT asset records. " +
            "Respond with a single concise sentence suitable for a business device catalog. No quotes or bullet points.";
        var user =
            $"Name: {device.Name}\nManufacturer: {device.Manufacturer}\nType: {typeLabel}\n" +
            $"OS: {device.OS} {device.OSVersion}\nRAM: {device.RamAmount}\nProcessor: {device.Processor}";

        var body = new ChatCompletionRequest(
            Model: _options.Model,
            Temperature: _options.Temperature,
            Messages:
            [
                new ChatMessage("system", system),
                new ChatMessage("user", user)
            ]);

        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var json = await res.Content.ReadAsStringAsync(cancellationToken);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"OpenAI error {(int)res.StatusCode}: {json}");

        using var doc = JsonDocument.Parse(json);
        var text = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return (text ?? string.Empty).Trim();
    }

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("messages")] ChatMessage[] Messages);
}
