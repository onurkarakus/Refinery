using Refinery.Core.Abstractions;
using Refinery.Infrastructure.Ai;
using Refinery.Infrastructure.Redis.Options;
using Refinery.Infrastructure.Redis.Services;
using Refinery.RefinementWorker;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
        builder.Services.AddSingleton<IRedisStreamService, RedisStreamService>();

        var geminiSettings = builder.Configuration.GetSection("Gemini");
        string apiKey = geminiSettings["ApiKey"] ?? throw new Exception("Gemini ApiKey bulunamadý!");
        string modelId = geminiSettings["ModelId"] ?? "gemini-1.5-flash";

        builder.Services.AddSingleton<IAiRefineryService>(sp => new AiRefineryGeminiService(apiKey, modelId));

        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}