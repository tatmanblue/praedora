namespace Praedora.Core.Entities;

public class EmailMessage
{
    public Guid Id { get; private set; }
    public string ExternalMessageId { get; private set; } = "";
    public string FromAddress { get; private set; } = "";
    public string Subject { get; private set; } = "";
    public DateTimeOffset ReceivedAt { get; private set; }
    public string BodyText { get; private set; } = "";
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? ClassificationResultJson { get; private set; }

    private EmailMessage()
    {
    }

    public static EmailMessage Create(
        string externalMessageId, string fromAddress, string subject,
        DateTimeOffset receivedAt, string bodyText)
    {
        return new EmailMessage
        {
            Id = Guid.NewGuid(),
            ExternalMessageId = externalMessageId,
            FromAddress = fromAddress,
            Subject = subject,
            ReceivedAt = receivedAt,
            BodyText = bodyText
        };
    }

    public void MarkProcessed(DateTimeOffset processedAt, string? classificationResultJson)
    {
        ProcessedAt = processedAt;
        ClassificationResultJson = classificationResultJson;
    }
}
