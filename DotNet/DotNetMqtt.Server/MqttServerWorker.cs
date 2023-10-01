using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;

namespace DotNetMqtt.Server;

public class MqttServerWorker : IHostedService, IDisposable
{
    private readonly ILogger<MqttServerWorker> _logger;
    private MqttServer? _mqttServer;

    public MqttServerWorker(ILogger<MqttServerWorker> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        _logger.LogInformation("Iniciando servidor MQTT");
        var mqttFactory = new MqttFactory();
        var optionsBuilder = new MqttServerOptionsBuilder()
                             .WithDefaultEndpoint();

        _mqttServer = mqttFactory.CreateMqttServer(optionsBuilder.Build());

        _mqttServer.ClientConnectedAsync += ClienteConectado;
        _mqttServer.ClientConnectedAsync += ClienteDesconectado;
        _mqttServer.InterceptingSubscriptionAsync += InterceptandoSubscripcion;
        _mqttServer.InterceptingUnsubscriptionAsync += InterceptandoDesuscripcion;
        _mqttServer.InterceptingPublishAsync += InterceptandoPublicacion;
        _mqttServer.InterceptingClientEnqueueAsync += InterceptandoColaCliente;
        await _mqttServer.StartAsync();
        _logger.LogInformation("Servidor MQTT iniciado");

    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if(_mqttServer != null)
        {
            _logger.LogInformation("Deteniendo servidor MQTT");
            await _mqttServer.StopAsync();
            _logger.LogInformation("Servidor MQTT detenido");
        }
    }


    private Task InterceptandoColaCliente(InterceptingClientApplicationMessageEnqueueEventArgs args)
    {
        _logger.LogInformation($"Cliente publicador {args.SenderClientId} suscriptor {args.ReceiverClientId} encola mensaje {args.ApplicationMessage.Topic} - {args.ApplicationMessage.ConvertPayloadToString()}");
        return Task.CompletedTask;
    }

    private Task InterceptandoPublicacion(InterceptingPublishEventArgs args)
    {
        _logger.LogInformation($"Cliente {args.ClientId} publicó en {args.ApplicationMessage.Topic} - {args.ApplicationMessage.ConvertPayloadToString()}");
        return Task.CompletedTask;
    }


    private Task InterceptandoDesuscripcion(InterceptingUnsubscriptionEventArgs args)
    {
       _logger.LogInformation($"Cliente {args.ClientId} se ha desuscrito a {args.Topic}");
        return Task.CompletedTask;
    }


    private Task InterceptandoSubscripcion(InterceptingSubscriptionEventArgs args)
    {
        _logger.LogInformation($"Cliente {args.ClientId} se ha suscrito a {args.TopicFilter.Topic}");
        return Task.CompletedTask;
    }


    private Task ClienteDesconectado(ClientConnectedEventArgs args)
    {
        _logger.LogInformation($"Cliente Desconectado {args.ClientId} {args.Endpoint}");
        return Task.CompletedTask;
    }


    private  Task ClienteConectado(ClientConnectedEventArgs args)
    {
        _logger.LogInformation($"Cliente conectado {args.ClientId} {args.Endpoint}");
        return Task.CompletedTask;
    }


    public void Dispose()
    {
        if(_mqttServer != null)
        {
            _mqttServer.ClientConnectedAsync -= ClienteConectado;
            _mqttServer.ClientConnectedAsync -= ClienteDesconectado;
            _mqttServer.InterceptingSubscriptionAsync -= InterceptandoSubscripcion;
            _mqttServer.InterceptingUnsubscriptionAsync -= InterceptandoDesuscripcion;
            _mqttServer.InterceptingPublishAsync -= InterceptandoPublicacion;
            _mqttServer.InterceptingClientEnqueueAsync -= InterceptandoColaCliente;
            _mqttServer.Dispose();
        }
    }
}
