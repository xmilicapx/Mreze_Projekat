using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core;
using Core.Models;

namespace Station
{
    internal class Program
    {
        private const int ServerPort = 51000;
        private const int UdpPort = 15000;
        private static Socket _tcpSocket;
        private static Socket _udpSocket;
        private static Core.Models.Station _stationData;

        public static void Main(string[] args)
        {
            InitializeStation();

            // Start device communication in a separate thread
            var deviceThread = new Thread(StartDeviceCommunication);
            deviceThread.Start();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        private static void InitializeStation()
        {
            try
            {
                // Initialize TCP connection to server
                _tcpSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                );

                // Connect to server
                var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), ServerPort);
                Console.WriteLine("Connecting to server...");
                _tcpSocket.Connect(serverEndPoint);
                Console.WriteLine("Connected to server!");

                // Receive initialization data
                var buffer = new byte[2048];
                _tcpSocket.Receive(buffer);
                _stationData = NetworkHelper.DeserializeObject<Core.Models.Station>(buffer);

                Console.WriteLine($"Initialized as station: {_stationData.Name}");
                Console.WriteLine($"Population: {_stationData.Population}");
                Console.WriteLine($"Device count: {_stationData.DeviceCount}");
                Console.WriteLine($"Location: {_stationData.Coordinates.Latitude}, {_stationData.Coordinates.Longitude}");

                // Initialize UDP socket for device communication
                _udpSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp
                );

                var localEndPoint = new IPEndPoint(IPAddress.Any, UdpPort);
                _udpSocket.Bind(localEndPoint);
                Console.WriteLine($"Listening for device connections on UDP port {UdpPort}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Station initialization error: {ex.Message}");
                _tcpSocket?.Close();
                _udpSocket?.Close();
                throw; // Re-throw to exit the application
            }
        }

        private static void SendMeasurementsToServer(Socket serverSocket, List<Measurement> measurements)
        {
            try
            {
                NetworkHelper.SendMessage(serverSocket, measurements);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending measurements: {ex.Message}");
            }
        }

        private static void StartDeviceCommunication()
        {
            var measurements = new List<Measurement>();
            var lastSendTime = DateTime.Now;

            while (true)
            {
                try
                {
                    var buffer = new byte[2048];
                    EndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);

                    // Non-blocking check for data
                    if (_udpSocket.Poll(1000000, SelectMode.SelectRead)) // 1 second timeout
                    {
                        _udpSocket.ReceiveFrom(buffer, ref remoteEp);
                        var measurement = NetworkHelper.DeserializeObject<Measurement>(buffer);
                        measurements.Add(measurement);
                        Console.WriteLine($"Received measurement from {measurement.DeviceId}:");
                        Console.WriteLine($"Type: {measurement.Type}");
                        Console.WriteLine($"Value: {measurement.Value} {measurement.Unit}");
                        Console.WriteLine($"Time: {measurement.Timestamp}");
                    }

                    // Send measurements to server every 5 seconds
                    if (measurements.Count > 0 && (DateTime.Now - lastSendTime).TotalSeconds >= 5)
                    {
                        SendMeasurementsToServer(_tcpSocket, measurements);
                        Console.WriteLine($"Sent {measurements.Count} measurements to server");
                        measurements.Clear();
                        lastSendTime = DateTime.Now;
                    }

                    Thread.Sleep(1000); // Wait 1 second before next check
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in device communication: {ex.Message}");
                    Thread.Sleep(5000); // Wait before retry
                }
            }
        }
    }
}
