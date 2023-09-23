#include <Arduino.h>

#include <WiFi.h>
#include <PubSubClient.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>

#define TOPIC_TEMPERATURA "esp32/temperatura"
#define TOPIC_HUMEDAD "esp32/humedad"

// Información de WiFi y MQTT
const char* ssid = "XXXXXXXXXXXXXXXXXXX";
const char* password = "XXXXXXXXXXXXXXX";
const char* mqttServer = "XXXXXXXXXXXXX";
const int mqttPort = 1883;

//Protipado de funciones
void callback(char*, byte*, unsigned int);

// Inicializa el cliente WiFi y MQTT
WiFiClient espClient;
PubSubClient client(espClient);

// Inicializa la LCD con dirección I2C, columnas y filas
LiquidCrystal_I2C lcd(0x27, 16, 2);

void setup() {
  // Inicializa el LCD
  lcd.init();
  lcd.backlight();
  
  // Conexión WiFi
  Serial.begin(115200);
  WiFi.begin(ssid, password);

  // Conexión WiFi
   while (true) { // Bucle infinito
    if (WiFi.status() == WL_CONNECTED) {
      Serial.println("Conectado a WiFi!");
      break; // Sale del bucle cuando se conecta
    }
    Serial.print(".");
    delay(200);
   }

  
  // Conexión MQTT
  client.setServer(mqttServer, mqttPort);
  client.setCallback(callback);

  while (!client.connected()) {
    Serial.println("Conectando a MQTT...");

    if (client.connect("ESP32")) {
      Serial.println("Conectado");
    } else {
      Serial.print("Error de conexión: ");
      Serial.print(client.state());
      delay(2000);
    }
  }

  Serial.println("Suscripción a tópicos");
  // Se suscribe al tópico
  client.subscribe(TOPIC_TEMPERATURA);
  client.subscribe(TOPIC_HUMEDAD);
}

void callback(char* topic, byte* payload, unsigned int length) {
  String payloadStr = "";

  Serial.println("mensaje recibido");

  // Convierte los bytes del payload a String
  for (int i = 0; i < length; i++) {
    payloadStr += (char)payload[i];
  }

  Serial.println(payloadStr);

  // Actualiza el LCD con el mensaje recibido
  if(strstr(topic,TOPIC_TEMPERATURA))
  {
    lcd.setCursor(0, 0);
    lcd.print("Temp. ");
  }

  if(strstr(topic,TOPIC_HUMEDAD)) 
  {
    lcd.setCursor(0, 1);
    lcd.print("Humd. ");
  }

  lcd.print(payloadStr);
}

void loop() {
  client.loop();
}