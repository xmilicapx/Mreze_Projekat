using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Core.Models;
using Core.Enums;

namespace Device
{
    internal class Program
    {
        private const int StationPort = 15000;
        private static Socket _udpSocket;
        private static Random _random = new Random();
        private static string _deviceId = "TEMP_001";

        public static void Main(string[] args)
        {
            StartDevice();
        }

        private static void StartDevice()
        {
            try
            {
                // Initialize UDP socket as per guide
                _udpSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp
                );

                IPEndPoint stationEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), StationPort);
                Console.WriteLine($"Device {_deviceId} started. Sending measurements to localhost:{StationPort}");

                while (true)
                {
                    // Create measurement with all required fields
                    var measurement = new Measurement
                    {
                        DeviceId = _deviceId,
                        Type = MeasurementType.Temperature,
                        Value = _random.Next(-10, 40), // Realistic temperature range
                        Unit = "°C",
                        Timestamp = DateTime.Now
                    };

                    // Serialize using BinaryFormatter as per guide
                    byte[] data;
                    using (var ms = new MemoryStream())
                    {
                        var bf = new BinaryFormatter();
                        bf.Serialize(ms, measurement);
                        data = ms.ToArray();
                    }

                    // Send using UDP
                    _udpSocket.SendTo(data, stationEndPoint);
                    Console.WriteLine($"Sent measurement: {measurement.Value} {measurement.Unit} at {measurement.Timestamp}");

                    Thread.Sleep(2000); // Wait between measurements
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Device error: {ex.Message}");
                _udpSocket?.Close();
            }
        }
    }
}
