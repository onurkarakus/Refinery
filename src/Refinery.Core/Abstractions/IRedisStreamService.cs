using Refinery.Core.Entities;

namespace Refinery.Core.Abstractions;
public interface IRedisStreamService
{
    Task PublishMailAsync(string streamKey, MailData mailData);

    Task ConsumeStreamAsync(
        string streamKey,
        string consumerGroup,
        string consumerName,
        Func<MailData, Task> processHandler,
        CancellationToken cancellationToken = default
        );

    Task CreateGroupAsync(string streamKey, string consumerGroup);
}
