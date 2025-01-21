using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using Parser.SensorReadings;
using Parser;

namespace TTN_MQTT_Labb3
{
    class Program
    {
        private static readonly string MQTT_HOST = "eu1.cloud.thethings.network";
        private static readonly int MQTT_PORT = 8883;
        private static readonly string USERNAME = "safiyo-applcation@ttn";
        private static readonly string PASSWORD = "NNSXS.43GP4YCGCFIXFQKKKE3K7QORHHZLUUWS6I6GUIY.OK4PHULALVJX7G6SHIQBIFWGWPROHG75Q2KM4QLWATMN5DEVSDYA";

        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTTnet ConsoleApp - A The Things Network V3 C# App");

            var mqttFactory = new MqttFactory();
            var mqttClient = mqttFactory.CreateMqttClient();

            var mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(MQTT_HOST, MQTT_PORT)
                .WithCredentials(USERNAME, PASSWORD)
                .WithTls()
                .Build();

            mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine("Connected to TTN V3 MQTT Broker: Success");
                string topicFilter = "v3/+/devices/+/up"; // TTN topic
                await mqttClient.SubscribeAsync(topicFilter);
                Console.WriteLine($"Subscribed to topic: {topicFilter}");
            };

            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string topic = e.ApplicationMessage.Topic;
                var payload = e.ApplicationMessage.Payload;

                Console.WriteLine($"Topic: {topic}");

                try
                {
                    // Parse JSON payload
                    var jsonDoc = JsonDocument.Parse(Encoding.UTF8.GetString(payload));
                    string frmPayload = jsonDoc.RootElement
                        .GetProperty("uplink_message")
                        .GetProperty("frm_payload")
                        .GetString();

                    // Decode Base64 payload
                    byte[] decodedPayload = Convert.FromBase64String(frmPayload);

                    Console.WriteLine($"Decoded Payload: {BitConverter.ToString(decodedPayload)}");

                    // Kontrollera payload-längd och välj dekodermetod
                    if (decodedPayload.Length == 12)
                    {
                        Console.WriteLine("Detected IEEE-754 payload.");
                        DecodeIEEE754Payload(decodedPayload);
                    }
                    else
                    {
                        Console.WriteLine("Detected Cayenne LPP payload.");
                        DecodeCayenneLPP(decodedPayload);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing or decoding payload: {ex.Message}");
                }

                return Task.CompletedTask;
            };

            try
            {
                await mqttClient.ConnectAsync(mqttOptions);
                Console.WriteLine("Connected to TTN V3: " + MQTT_HOST);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                return;
            }

            Console.WriteLine("Waiting for messages...");
            Console.ReadLine();
        }

        // IEEE-754 Payload Decoding
        public static void DecodeIEEE754Payload(byte[] payload)
        {
            if (payload.Length != 12)
            {
                Console.WriteLine($"Unexpected payload length: {payload.Length} bytes. Ignoring message.");
                return;
            }

            try
            {
                bool isLittleEndian = BitConverter.IsLittleEndian;

                // Temperatur (4 bytes)
                byte[] temperatureBytes = payload[0..4];
                if (isLittleEndian) Array.Reverse(temperatureBytes);
                float temperature = BitConverter.ToSingle(temperatureBytes, 0);

                // Fuktighet (4 bytes)
                byte[] humidityBytes = payload[4..8];
                if (isLittleEndian) Array.Reverse(humidityBytes);
                float humidity = BitConverter.ToSingle(humidityBytes, 0);

                // LED-ljusstyrka (4 bytes)
                byte[] ledBytes = payload[8..12];
                if (isLittleEndian) Array.Reverse(ledBytes);
                float ledBrightness = BitConverter.ToSingle(ledBytes, 0);

                Console.WriteLine($"Decoded IEEE-754 Payload: Temperature = {temperature:F2}, Humidity = {humidity:F2}, LED Brightness = {ledBrightness:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decoding IEEE-754 payload: {ex.Message}");
            }
        }

        // Cayenne LPP Payload Decoding
        public static void DecodeCayenneLPP(byte[] payload)
        {
            try
            {
                var sensorReadings = SensorReadingParser.Parse(payload);

                foreach (var reading in sensorReadings)
                {
                    switch (reading)
                    {
                        case TemperatureSensor temperature:
                            Console.WriteLine($"CayenneLPP Payload: Temperature = {temperature.Temperature:F2}");
                            break;

                        case HumiditySensor humidity:
                            Console.WriteLine($"Humidity = {humidity.Humidity:F2}");
                            break;

                        case AnalogOutput led:
                            Console.WriteLine($"LED Value = {led.Value:F2}");
                            break;

                        case GpsLocation gps:
                            Console.WriteLine($"Device Location: Latitude = {gps.Latitude:F6}, Longitude = {gps.Longitude:F6}, Altitude = {gps.Altitude:F0}m");
                            break;

                        default:
                            Console.WriteLine($"Unknown sensor type: {reading.GetType().Name}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decoding Cayenne LPP payload: {ex.Message}");
            }
        }
    }
}
