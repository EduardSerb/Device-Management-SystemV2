namespace DeviceManagement.Api.Options;

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>API key for OpenAI-compatible endpoints.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Base URL, e.g. https://api.openai.com/v1/ or local Ollama OpenAI bridge.</summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

    public string Model { get; set; } = "gpt-4o-mini";

    public double Temperature { get; set; } = 0.35;
}
