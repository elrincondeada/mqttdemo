using MQTTnet;

internal class Program
{
    private static void Main(string[] args)
    {

        var mqttFactory = new MqttFactory();

          using (var mqttClient = mqttFactory.CreateMqttClient())
          {

          }
    }
}