using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using MQTTnet;
using MQTTnet.Client;

namespace DotNetMqtt.Avalonia.App
{
    public partial class MainWindow : Window
    {

        private readonly IMqttClient _mqttClient;
        private const string HOST_MQTT = "localhost";

        public MainWindow()
        {
            InitializeComponent();
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();
            _ = Init();
        }


        public async Task Init()
        {
            await SuscribirMqtt();
        }

        public async Task SuscribirMqtt(CancellationToken cancellationToken = default)
        {
            var mqttFactory = new MqttFactory();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                                        .WithTcpServer(HOST_MQTT).Build();

            //Evento que se va a lanzar cada vez que se publique en uno de
            //los "topic" a los que estamos suscritos
            _mqttClient.ApplicationMessageReceivedAsync += OnMensajeRecibido;

            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttClientOpcionTemperatura = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("esp32/temperatura");
                    })
                .Build();

            var mqttClientOpcionHumedad = mqttFactory.CreateSubscribeOptionsBuilder()
               .WithTopicFilter(
                   f =>
                   {
                       f.WithTopic("esp32/humedad");
                   })
               .Build();

            await _mqttClient.SubscribeAsync(mqttClientOpcionTemperatura, CancellationToken.None);
            await _mqttClient.SubscribeAsync(mqttClientOpcionHumedad, CancellationToken.None);

            Console.WriteLine("MQTT cliente suscripto a los \"topic\".");

            while (true)
            {
                await Task.Delay(2000);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }


        public async Task OnMensajeRecibido(MqttApplicationMessageReceivedEventArgs e)
        {
            var valor = Encoding.ASCII.GetString(e.ApplicationMessage.PayloadSegment);
            Console.WriteLine($"Recibido mensaje. topic:{e.ApplicationMessage.Topic}");
            Console.WriteLine(valor);
            if (e.ApplicationMessage.Topic == "esp32/temperatura")
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.FindControl<TextBlock>("TemperaturaLabel").Text = valor;
                });
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.FindControl<TextBlock>("HumedadLabel").Text = valor;
                });
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            _mqttClient.ApplicationMessageReceivedAsync -= OnMensajeRecibido;
            _mqttClient.Dispose();
        }
    }
}
