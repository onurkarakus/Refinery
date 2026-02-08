using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refinery.Core.Abstractions;
using Refinery.Core.Entities;
using Refinery.Infrastructure.Redis.Options;
using StackExchange.Redis;

namespace Refinery.Infrastructure.Redis.Services;

public class RedisStreamService : IRedisStreamService
{
    private readonly ConnectionMultiplexer redis;
    private readonly IDatabase database;
    private readonly IOptions<RedisOptions> options;
    private readonly ILogger<RedisStreamService> logger;

    public RedisStreamService(IOptions<RedisOptions> options, ILogger<RedisStreamService> logger)
    {
        this.logger = logger;
        this.options = options;
        this.redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { $"{options.Value.HostName}:{options.Value.Port},password={options.Value.Password}" },
            Password = options.Value.Password,
            Ssl = options.Value.Ssl,
        });

        this.database = redis.GetDatabase();
    }

    public async Task ConsumeStreamAsync(string streamKey, string consumerGroup, string consumerName, Func<MailData, Task> processHandler, CancellationToken cancellationToken = default)
    {
        var results = await database.StreamReadGroupAsync(streamKey, consumerGroup, consumerName, ">", count: 1);
        
        if (results.Any())
        {
            var entry = results.First();
            var mailData = MapToMailData(entry);

            try
            {
                await processHandler(mailData);

                await database.StreamAcknowledgeAsync(streamKey, consumerGroup, entry.Id);
            }
            catch (Exception)
            {
                logger.LogError("Error processing stream entry with ID {EntryId} in stream {StreamKey} for consumer group {ConsumerGroup} and consumer {ConsumerName}", 
                    entry.Id, 
                    streamKey, 
                    consumerGroup, 
                    consumerName);

                throw;
            }
        }        
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
