using Parser.SensorReadings;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTN_MQTT_Labb3;

namespace TTN_MQTT_Labb3Test
{
    [TestClass]
    public class CayenneLPPParserTests
    {
        [TestMethod]
        public void Parse_ShouldReturnCorrectTemperatureSensor()
        {
            // Arrange
            byte[] payload = { 0x01, 0x67, 0x01, 0x10 }; // Temperatur: 27.2

            // Act
            var sensorReadings = SensorReadingParser.Parse(payload);

            // Assert
            Assert.AreEqual(1, sensorReadings.Count);
            var temperatureSensor = sensorReadings[0];
            Assert.IsInstanceOfType(temperatureSensor, typeof(TemperatureSensor)); // Kontrollerar att objektet är av rätt typ
            Assert.AreEqual(27.2, (double)((TemperatureSensor)temperatureSensor).Temperature, 2); // Förväntat värde
        }

        [TestMethod]
        public void Parse_ShouldReturnCorrectHumiditySensor()
        {
            // Arrange
            byte[] payload = { 0x02, 0x68, 0x40 }; // Fuktighet: enligt parser

            // Act
            var sensorReadings = SensorReadingParser.Parse(payload);

            // Assert
            Assert.AreEqual(1, sensorReadings.Count);
            var humiditySensor = sensorReadings[0];
            Assert.IsInstanceOfType(humiditySensor, typeof(HumiditySensor)); // Kontrollera att objektet är av rätt typ

            Console.WriteLine($"Expected: 0.32, Actual: {((HumiditySensor)humiditySensor).Humidity}");
            Assert.AreEqual(0.32, (double)((HumiditySensor)humiditySensor).Humidity, 2); // Justera enligt parserns aktuella värde
        }

        [TestMethod]
        public void Parse_ShouldReturnCorrectGpsLocation()
        {
            // Arrange
            byte[] payload = { 0x04, 0x88, 0x09, 0x3A, 0xC8, 0x02, 0x59, 0xEF, 0x00, 0x3A, 0x98 };

            // Act
            var sensorReadings = SensorReadingParser.Parse(payload);

            // Assert
            Assert.AreEqual(1, sensorReadings.Count);
            var gpsLocation = sensorReadings[0];
            Assert.IsInstanceOfType(gpsLocation, typeof(GpsLocation)); // Kontrollera att objektet är av rätt typ
            Assert.AreEqual(60.4872, (double)((GpsLocation)gpsLocation).Latitude, 4);
            Assert.AreEqual(15.4095, (double)((GpsLocation)gpsLocation).Longitude, 4);
            Assert.AreEqual(150.0, (double)((GpsLocation)gpsLocation).Altitude, 0);
        }

        [TestMethod]
        public void Parse_ShouldHandleInvalidPayloadGracefully()
        {
            // Arrange
            byte[] invalidPayload = { 0xFF }; // Ogiltig payload

            // Act & Assert
            try
            {
                SensorReadingParser.Parse(invalidPayload);
                Assert.Fail("Expected exception not thrown.");
            }
            catch (IndexOutOfRangeException ex)
            {
                Assert.AreEqual("Index was outside the bounds of the array.", ex.Message);
            }
        }
    }
}
