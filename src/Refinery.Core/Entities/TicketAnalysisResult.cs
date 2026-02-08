namespace Refinery.Core.Entities;

public class TicketAnalysisResult
{
    public string Summary { get; set; }

    public string Category { get; set; } = "General";

    public string Urgency { get; set; } = "Low";

    public bool MissingInfo { get; set; }

    public string MissingInfoDetails { get; set; }

    public string? Sentiment { get; set; }
}
