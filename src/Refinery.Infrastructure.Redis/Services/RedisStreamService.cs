using Microsoft.Extensions.Options;
using Refinery.Core.Entities;
using Refinery.Infrastructure.Redis.Abstractions;
using Refinery.Infrastructure.Redis.Options;
using StackExchange.Redis;

namespace Refinery.Infrastructure.Redis.Services;

public class RedisStreamService : IRedisStreamService
{
    private readonly ConnectionMultiplexer redis;
    private readonly IDatabase database;
    private readonly IOptions<RedisOptions> options;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly CancellationToken cancellationToken;

    public RedisStreamService(IOptions<RedisOptions> options)
    {
        this.options = options;
        this.redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { $"{options.Value.HostName}:{options.Value.Port},password={options.Value.Password}" },
            Password = options.Value.Password,
            Ssl = options.Value.Ssl,
        });

        this.database = redis.GetDatabase();

        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;
    }

    public async Task ConsumeStreamAsync(string streamKey, string consumerGroup, string consumerName, Func<MailData, Task> processHandler, CancellationToken cancellationToken = default)
    {
        var result = await database.StreamReadGroupAsync(streamKey, consumerGroup, consumerName, ">", count: 10);
        
        var mailData = result.Select(MapToMailData);

        await processHandler(mailData.FirstOrDefault()!);
        await database.StreamAcknowledgeAsync(streamKey, consumerGroup, result.FirstOrDefault().Id);
    }

    public async Task CreateGroupAsync(string streamKey, string consumerGroup)
    {
        if (!await database.KeyExistsAsync(streamKey) || (await database.StreamGroupInfoAsync(streamKey)).All(x => x.Name != consumerGroup))
        {
            await database.StreamCreateConsumerGroupAsync(streamKey, consumerGroup, "0-0", true);
        }
    }

    public async Task PublishMailAsync(string streamKey, MailData mailData)
    {
        await database.StreamAddAsync(streamKey, new NameValueEntry[]
        {
            new("Subject", mailData.Subject),
            new("Body", mailData.Body),
            new("Sender", mailData.Sender),
            new("Recipient", mailData.Recipient)
        });
    }

    private MailData MapToMailData(StreamEntry entry)
    {
        var resultDictionary = entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

        return new MailData
        {
            Subject = resultDictionary.ContainsKey("Subject") ? resultDictionary["Subject"] : string.Empty,
            Body = resultDictionary.ContainsKey("Body") ? resultDictionary["Body"] : string.Empty,
            Sender = resultDictionary.ContainsKey("Sender") ? resultDictionary["Sender"] : string.Empty,
            Recipient = resultDictionary.ContainsKey("Recipient") ? resultDictionary["Recipient"] : string.Empty
        };
    }
}
