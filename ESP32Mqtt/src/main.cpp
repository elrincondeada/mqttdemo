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

#include <Arduino.h>
#include <Adafruit_AHTX0.h>
#include <WiFi.h>
#include <PubSubClient.h>

Adafruit_AHTX0 aht;

const char *ssid = "XXXXXXXXXXXXXXXXXXXXXXX";
const char *password = "XXXXXXXXXXXXXXXXXXX";
const char *mqtt_server ="XXXXXXXXXXXXXXXXX";
int mqtt_port = 1883;

WiFiClient espClient;
PubSubClient client(espClient);
void reconnect();

void setup()
{
  Serial.begin(9600);
  Serial.println("Demo mqtt con AHT20");
  if (!aht.begin())
  {
    Serial.println("No encontró el sensor AHT?");
    while (1)
      delay(10);
  }
  Serial.println("AHT20 encontrado!");

  // Conectar a WiFi
  Serial.println();
  Serial.print("Conectando a ");
  Serial.println(ssid);

  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }

  Serial.println();
  Serial.println("WiFi conectado");
  Serial.println("Dirección IP: ");
  Serial.println(WiFi.localIP());

  // Configurar cliente MQTT
  client.setServer(mqtt_server, mqtt_port);
}


void loop()
{
  sensors_event_t e_humedad, e_temp;
  aht.getEvent(&e_humedad, &e_temp); // populate temp and humidity objects with fresh data
  Serial.print("Temperature: ");
  Serial.print(e_temp.temperature);
  Serial.println(" degrees C");
  Serial.print("Humidity: ");
  Serial.print(e_humedad.relative_humidity);
  Serial.println("% rH");

  if (!client.connected())
  {
    reconnect();
  }
  client.loop();

  long now = millis();
  static long lastMsg = 0;

  if (now - lastMsg > 10000)
  { // Enviar telemetría cada 10 segundos
    lastMsg = now;

    // Aquí, pon el código para leer tu sensor
    float temperatura = roundf(e_temp.temperature * 10)/10;
    float humedad = roundf(e_humedad.relative_humidity);

    char msgt[50];
    snprintf(msgt, 50, "%.1f C", temperatura);
    Serial.print("Publicando mensaje: ");
    Serial.println(msgt);

    // Publicar el mensaje
    client.publish("esp32/temperatura", msgt);

    char msgh[50];
    snprintf(msgh, 50, "%.0f %%", humedad);
    Serial.print("Publicando mensaje: ");
    Serial.println(msgh);

    // Publicar el mensaje
    client.publish("esp32/humedad", msgh);
  }

  delay(500);
}


void reconnect()
{
  // Loop hasta que estemos reconectados
  while (!client.connected())
  {
    Serial.print("Intentando conexión MQTT...");
    // Intentar conectar
    if (client.connect("ESP32Client"))
    {
      Serial.println("conectado");
    }
    else
    {
      Serial.print("falló, rc=");
      Serial.print(client.state());
      Serial.println(" intentar de nuevo en 5 segundos");
      // Esperar 5 segundos antes de volver a intentar
      delay(5000);
    }
  }
}
