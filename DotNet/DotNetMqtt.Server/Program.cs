namespace DotNetMqtt.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args);
        hostBuilder.ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<MqttServerWorker>();
        });
        await hostBuilder.RunConsoleAsync();
    }
}
