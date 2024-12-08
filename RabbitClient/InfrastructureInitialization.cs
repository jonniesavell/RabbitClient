namespace RabbitClient;

using RabbitMQ.Client;

class InfrastructureInitialization
{
    static async Task Main(string[] args)
    {
        string? host = Environment.GetEnvironmentVariable("HOST");
        string? port = Environment.GetEnvironmentVariable("PORT");
        int portNumber;
        if (!int.TryParse(port, out portNumber))
        {
            portNumber = 5672;
        }

        string? username = Environment.GetEnvironmentVariable("USERNAME");
        string? password = Environment.GetEnvironmentVariable("PASSWORD");

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = portNumber,
            UserName = username,
            Password = password
        };

        using var connection = await factory.CreateConnectionAsync();
        {
            using var channel = await connection.CreateChannelAsync();
            {
                await channel.ExchangeDeclareAsync(
                    exchange: "direct-to-hole",
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>()
                );

                await channel.ExchangeDeclareAsync(
                    exchange: "direct-to-sink",
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>()
                );

                await channel.ExchangeDeclareAsync(
                    exchange: "open",
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>()
                );

                await channel.QueueDeclareAsync(
                    queue: "hole",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>()
                );

                await channel.QueueDeclareAsync(
                    queue: "sink",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>()
                );

                await channel.QueueBindAsync(
                    queue: "hole",
                    exchange: "direct-to-hole",
                    routingKey: "routing-key",
                    arguments: new Dictionary<string, object?>()
                );

                await channel.QueueBindAsync(
                    queue: "sink",
                    exchange: "direct-to-sink",
                    routingKey: "routing-key",
                    arguments: new Dictionary<string, object?>()
                );

                await channel.QueueBindAsync(
                    queue: "hole",
                    exchange: "open",
                    routingKey: ""
                );

                await channel.QueueBindAsync(
                    queue: "sink",
                    exchange: "open",
                    routingKey: ""
                );
            }
        }
    }
}
