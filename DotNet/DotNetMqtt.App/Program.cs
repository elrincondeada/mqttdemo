/*
MIT License

Copyright (c) [2023] [Ricardo Ortega. El Rincón de Ada. programación]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Text;
using MQTTnet;
using MQTTnet.Client;

internal class Program
{

    //recuerda cambiar por la ip o dirección del broker
    public const string HOST = "localhost";

    private static async Task Main(string[] args)
    {
        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                                        .WithTcpServer(HOST).Build();


            //Evento que se va a lanzar cada vez que se publique en uno de
            //los "topic" a los que estamos suscritos
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine($"Recibido mensaje. topic:{e.ApplicationMessage.Topic}");
                Console.WriteLine(Encoding.ASCII.GetString(e.ApplicationMessage.PayloadSegment));
                return Task.CompletedTask;
            };

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

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

            await mqttClient.SubscribeAsync(mqttClientOpcionTemperatura, CancellationToken.None);
            await mqttClient.SubscribeAsync(mqttClientOpcionHumedad, CancellationToken.None);

            Console.WriteLine("MQTT cliente suscripto a los \"topic\".");

            Console.WriteLine("Pulsa Enter para salir");
            Console.ReadLine();
        }
    }
}