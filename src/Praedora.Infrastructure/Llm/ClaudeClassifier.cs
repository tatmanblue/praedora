using System.Text.Json;
using System.Text.Json.Serialization;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using Praedora.Core.Entities;
using Praedora.Core.Enums;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Llm;

// Implements ILlmClassifier via the Claude API (design doc §3, §7), using structured output
// (OutputConfig.Format) so the response deserializes directly into EmailClassification.
public class ClaudeClassifier(IConfiguration configuration) : ILlmClassifier
{
    private const int MaxBodyChars = 6000;

    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<EmailClassification> ClassifyAsync(EmailMessage email, CancellationToken ct)
    {
        string apiKey = configuration["CLAUDE_API_KEY"] is { Length: > 0 } key
            ? key
            : throw new InvalidOperationException("CLAUDE_API_KEY is not set. Copy .env.example to .env and fill it in.");
        string model = configuration["CLAUDE_MODEL"] is { Length: > 0 } configuredModel ? configuredModel : "claude-opus-5";

        AnthropicClient client = new() { ApiKey = apiKey };

        string body = email.BodyText.Length > MaxBodyChars ? email.BodyText[..MaxBodyChars] : email.BodyText;

        MessageCreateParams parameters = new()
        {
            Model = model,
            MaxTokens = 1024,
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
                    Content = "Is this email related to a job application the recipient submitted? If so, which " +
                              "company is it from and what does it imply about the application's status? Only set " +
                              "InferredStatus when the email clearly implies one of the listed values — leave it " +
                              "null if the email is ambiguous. RawSnippet should be the specific sentence that " +
                              "justifies your answer. Confidence must be a number between 0 and 1.\n\n" +
                              $"From: {email.FromAddress}\nSubject: {email.Subject}\n\n{body}"
                }
            ]
        };

        Message response = await client.Messages.Create(parameters, cancellationToken: ct);

        TextBlock textBlock = response.Content
            .Select(block => block.Value)
            .OfType<TextBlock>()
            .First();

        return JsonSerializer.Deserialize<EmailClassification>(textBlock.Text, ResponseJsonOptions)
            ?? throw new InvalidOperationException("Claude returned an empty email classification.");
    }

    private static Dictionary<string, JsonElement> BuildSchema()
    {
        string[] statusNames = Enum.GetNames<ApplicationStatus>();

        return new Dictionary<string, JsonElement>
        {
            ["type"] = JsonSerializer.SerializeToElement("object"),
            ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
            ["properties"] = JsonSerializer.SerializeToElement(new
            {
                IsJobRelated = new { type = "boolean" },
                InferredCompanyName = new { type = new[] { "string", "null" } },
                InferredStatus = new
                {
                    anyOf = new object[]
                    {
                        new { type = "string", @enum = statusNames },
                        new { type = "null" }
                    }
                },
                Confidence = new { type = "number" },
                Reasoning = new { type = new[] { "string", "null" } },
                RawSnippet = new { type = new[] { "string", "null" } }
            }),
            ["required"] = JsonSerializer.SerializeToElement(new[]
            {
                "IsJobRelated", "InferredCompanyName", "InferredStatus", "Confidence", "Reasoning", "RawSnippet"
            })
        };
    }
}
