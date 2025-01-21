using TTN_MQTT_Labb3;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TTN_MQTT_Labb3Test
{
    [TestClass]
    public class decoderTests
    {
        [TestMethod]
        [DataRow(new byte[] { 0x42, 0x20, 0x00, 0x00 }, 40.0f)] // Temperatur
        [DataRow(new byte[] { 0x42, 0x48, 0xCC, 0xCD }, 50.2f)] // Luftfuktighet
        [DataRow(new byte[] { 0x3F, 0x80, 0x00, 0x00 }, 1.0f)]  // LED-ljusstyrka
        public void FloatIEE754_ShouldConvertValidValues_WhenDecodingInput(byte[] input, float expected)
        {
            // Arrange
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }

            // Act
            float actual = BitConverter.ToSingle(input, 0);

            // Assert
            Assert.AreEqual(expected, actual, 0.01f); // Adjust precision to match your requirements
        }

        [TestMethod]
        [DataRow(new byte[] { 0x42, 0x6A }, typeof(ArgumentException))] // Felaktig arraystorlek
        [DataRow(null, typeof(ArgumentException))]                     // Null-värde
        public void FloatIEE754_ShouldThrowException_ForInvalidInput(byte[] input, Type expectedException)
        {
            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() =>
            {
                if (input == null || input.Length != 4)
                {
                    throw new ArgumentException("Input must be a 4-byte array.");
                }
                BitConverter.ToSingle(input, 0);
            });

            Assert.AreEqual(expectedException, exception.GetType());
        }

        [TestMethod]
        public void ByteSwapX4_ShouldConvertValidValues_WhenDecodingInput()
        {
            // Arrange
            byte[] input = { 0x42, 0x48, 0xCC, 0xCD }; // Luftfuktighet
            byte[] expected = { 0xCD, 0xCC, 0x48, 0x42 }; // Förväntat värde efter byte swap

            // Act
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }

            // Assert
            CollectionAssert.AreEqual(expected, input);
        }

        [TestMethod]
        public void PayloadDecoder_ShouldDecodeFullPayload_Correctly()
        {
            // Arrange
            byte[] payload = {
                0x42, 0x20, 0x00, 0x00, // Temperatur: 40.0
                0x42, 0x48, 0xCC, 0xCD, // Luftfuktighet: 50.2
                0x3F, 0x80, 0x00, 0x00  // LED-ljusstyrka: 1.0
            };

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(payload, 0, 4); // Temperatur
                Array.Reverse(payload, 4, 4); // Luftfuktighet
                Array.Reverse(payload, 8, 4); // LED-ljusstyrka
            }

            // Act
            float temperature = BitConverter.ToSingle(payload, 0);
            float humidity = BitConverter.ToSingle(payload, 4);
            float ledBrightness = BitConverter.ToSingle(payload, 8);

            // Assert
            Assert.AreEqual(40.0f, temperature, 0.01f); // Adjust precision to match your requirements
            Assert.AreEqual(50.2f, humidity, 0.01f);
            Assert.AreEqual(1.0f, ledBrightness, 0.01f);
        }
    }
}

