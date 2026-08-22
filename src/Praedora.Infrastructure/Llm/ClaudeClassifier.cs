using Praedora.Core.Entities;
using Praedora.Core.Interfaces;

namespace Praedora.Infrastructure.Llm;

// Implements ILlmClassifier via the Claude API (design doc §3, §7). Deferred to build
// sequence step 4 — will read CLAUDE_API_KEY from .env.
public class ClaudeClassifier : ILlmClassifier
{
    public Task<EmailClassification> ClassifyAsync(EmailMessage email, CancellationToken ct)
    {
        throw new NotImplementedException("Email classification arrives in build sequence step 4.");
    }
}
