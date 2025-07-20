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
        private const int BASE_STATION_PORT = 15000;
        private static Socket _udpSocket;
        private static string _deviceId;
        private static IPEndPoint _stationEndPoint;
        private static MeasurementType _measurementType;

        public static void Main(string[] args)
        {
            ConfigureDevice();
            StartDevice();
        }

        private static void ConfigureDevice()
        {
            Console.WriteLine("Enter device ID (e.g., TEMP_001):");
            _deviceId = Console.ReadLine() ?? "TEMP_001";

            Console.WriteLine("Available stations:");
            Console.WriteLine("1. Station 1 (Port 15000)");
            Console.WriteLine("2. Station 2 (Port 15001)");
            Console.WriteLine("3. Station 3 (Port 15002)");
            Console.Write("Select station (1-3): ");

            int stationChoice;
            while (!int.TryParse(Console.ReadLine(), out stationChoice) || stationChoice < 1 || stationChoice > 3)
            {
                Console.Write("Invalid choice. Please select 1-3: ");
            }

            int stationPort = BASE_STATION_PORT + (stationChoice - 1);
            _stationEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), stationPort);

            Console.WriteLine("Available measurement types:");
            Console.WriteLine("1. Temperature");
            Console.WriteLine("2. Humidity");
            Console.WriteLine("3. Wind Speed");
            Console.WriteLine("4. Wind Direction");
            Console.WriteLine("5. Pressure");
            Console.WriteLine("6. Precipitation");
            Console.WriteLine("7. Chemical Composition");
            Console.WriteLine("8. Cloudiness");
            Console.Write("Select measurement type (1-8): ");

            int measurementChoice;
            while (!int.TryParse(Console.ReadLine(), out measurementChoice) || measurementChoice < 1 || measurementChoice > 8)
            {
                Console.Write("Invalid choice. Please select 1-8: ");
            }

            _measurementType = (MeasurementType)(measurementChoice - 1);

            Console.WriteLine($"Device {_deviceId} will connect to station at port {stationPort} and measure {_measurementType}");
        }

        private static void StartDevice()
        {
            try
            {
                _udpSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp
                );

                Console.WriteLine($"Device {_deviceId} started. Sending {_measurementType} measurements to {_stationEndPoint}");

                while (true)
                {
                    var measurement = MeasurementGenerator.GenerateMeasurement(_measurementType, _deviceId);
                    SendMeasurement(measurement);

                    // Check for abnormal values and send alarms
                    CheckAndSendAlarms(measurement);

                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Device error: {ex.Message}");
                _udpSocket?.Close();
            }
        }

        private static void SendMeasurement(Measurement measurement)
        {
            try
            {
                byte[] data;
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, measurement);
                    data = ms.ToArray();
                }

                _udpSocket.SendTo(data, _stationEndPoint);
                Console.WriteLine($"Sent measurement: {measurement.Value} {measurement.Unit} at {measurement.Timestamp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending measurement: {ex.Message}");
            }
        }

        private static void CheckAndSendAlarms(Measurement measurement)
        {
            var alarms = MeasurementGenerator.CheckForAlarms(measurement);

            foreach (var alarm in alarms)
            {
                try
                {
                    byte[] alarmData;
                    using (var ms = new MemoryStream())
                    {
                        var bf = new BinaryFormatter();
                        bf.Serialize(ms, alarm);
                        alarmData = ms.ToArray();
                    }

                    _udpSocket.SendTo(alarmData, _stationEndPoint);
                    Console.WriteLine($"ALARM sent: {alarm.Cause}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending alarm: {ex.Message}");
                }
            }
        }
    }
}