using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Llm;

// Implements IJobDescriptionExtractor via the Claude API (design doc §3, §7), using structured
// output (OutputConfig.Format) so the response deserializes directly into JobDescriptionExtract
// rather than relying on free-text parsing.
public class ClaudeJobDescriptionExtractor(IConfiguration configuration) : IJobDescriptionExtractor
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<JobDescriptionExtract> ExtractAsync(string rawJobDescription, CancellationToken ct)
    {
        string apiKey = configuration["CLAUDE_API_KEY"] is { Length: > 0 } key
            ? key
            : throw new InvalidOperationException("CLAUDE_API_KEY is not set. Copy .env.example to .env and fill it in.");
        string model = configuration["CLAUDE_MODEL"] is { Length: > 0 } configuredModel ? configuredModel : "claude-opus-5";

        AnthropicClient client = new() { ApiKey = apiKey };

        MessageCreateParams parameters = new()
        {
            Model = model,
            MaxTokens = 2048,
            OutputConfig = new OutputConfig
            {
                Effort = Effort.Low,
                Format = new JsonOutputFormat { Schema = BuildSchema() }
            },
            Messages =
            [
                new()
                {
                    Role = Role.User,
                    Content = "Extract structured fields from this job description. If a field " +
                              "is not present in the text, return null for it (or an empty array " +
                              $"for the skill lists).\n\n---\n\n{rawJobDescription}"
                }
            ]
        };

        Message response = await client.Messages.Create(parameters, cancellationToken: ct);

        TextBlock textBlock = response.Content
            .Select(block => block.Value)
            .OfType<TextBlock>()
            .First();

        return JsonSerializer.Deserialize<JobDescriptionExtract>(textBlock.Text, ResponseJsonOptions)
            ?? throw new InvalidOperationException("Claude returned an empty job description extract.");
    }

    private static Dictionary<string, JsonElement> BuildSchema()
    {
        return new Dictionary<string, JsonElement>
        {
            ["type"] = JsonSerializer.SerializeToElement("object"),
            ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
            ["properties"] = JsonSerializer.SerializeToElement(new
            {
                RequiredSkills = new { type = "array", items = new { type = "string" } },
                NiceToHave = new { type = "array", items = new { type = "string" } },
                SalaryRangeText = new { type = new[] { "string", "null" } },
                Location = new { type = new[] { "string", "null" } },
                RemotePolicy = new { type = new[] { "string", "null" } },
                YearsExperience = new { type = new[] { "integer", "null" } }
            }),
            ["required"] = JsonSerializer.SerializeToElement(new[]
            {
                "RequiredSkills", "NiceToHave", "SalaryRangeText", "Location", "RemotePolicy", "YearsExperience"
            })
        };
    }
}
