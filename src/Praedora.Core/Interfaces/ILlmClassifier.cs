using Praedora.Core.Entities;
using Praedora.Core.Enums;

namespace Praedora.Core.Interfaces;

public interface ILlmClassifier
{
    Task<EmailClassification> ClassifyAsync(EmailMessage email, CancellationToken ct);
}

public record EmailClassification(
    bool IsJobRelated,
    string? InferredCompanyName,
    ApplicationStatus? InferredStatus,
    double Confidence,
    string? Reasoning,
    string? RawSnippet);
