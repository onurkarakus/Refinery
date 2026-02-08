using Refinery.Infrastructure.Redis.Abstractions;
using Refinery.Infrastructure.Redis.Options;
using Refinery.Infrastructure.Redis.Services;
using Refinery.RefinementWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.AddSingleton<IRedisStreamService, RedisStreamService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
