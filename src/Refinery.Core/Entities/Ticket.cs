namespace Refinery.Core.Entities;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Sender { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Urgency { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public bool MissingInfo { get; set; }

    public string MissingInfoDetails { get; set; } = string.Empty;

    public string? AiSentiment { get; set; }

    public string Status { get; set; } = "New";
}
