using Refinery.Core.Entities;

namespace Refinery.Core.Abstractions;

public interface IAiRefineryService
{
    Task<TicketAnalysisResult> RefineMailAsync(MailData mailData);
}
