namespace Refinery.Infrastructure.Redis.Options;

public class RedisOptions
{
    public string HostName { get; set; } = "localhost";

    public string Port { get; set; } = "6379";

    public string Password { get; set; } = string.Empty;

    public bool Ssl { get; set; } = false;
}
