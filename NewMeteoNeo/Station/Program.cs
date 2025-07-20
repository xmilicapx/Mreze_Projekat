using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Core.Models;

namespace Station
{
    internal class Program
    {
        private const int SERVER_PORT = 10000;
        private const string SERVER_IP = "127.0.0.1";
        private static Socket _tcpSocket;
        private static Socket _udpSocket;
        private static List<Measurement> _measurements = new List<Measurement>();
        private static List<Alarm> _activeAlarms = new List<Alarm>();
        private static Core.Models.Station _stationInfo;

        public static void Main(string[] args)
        {
            Console.WriteLine("Enter station port (15000-15002):");
            int stationPort;
            while (!int.TryParse(Console.ReadLine(), out stationPort) ||
                   stationPort < 15000 || stationPort > 15002)
            {
                Console.WriteLine("Invalid port. Please enter a port between 15000-15002:");
            }

            InitializeStation(stationPort);
            StartStation();
        }

        private static void InitializeStation(int udpPort)
        {
            try
            {
                // Initialize TCP connection to server
                _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _tcpSocket.Connect(new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT));

                // Initialize UDP listener for devices
                _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _udpSocket.Bind(new IPEndPoint(IPAddress.Any, udpPort));

                // Get station info from server using the new method
                _stationInfo = NetworkHelper.ReceiveMessage<Core.Models.Station>(_tcpSocket);

                Console.WriteLine($"Station initialized: {_stationInfo.Name}");
                Console.WriteLine($"Listening for devices on UDP port {udpPort}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Initialization error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static void StartStation()
        {
            // Start device listener thread
            var deviceThread = new Thread(ListenForDevices);
            deviceThread.Start();

            // Start server communication thread
            var serverThread = new Thread(CommunicateWithServer);
            serverThread.Start();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();

            _tcpSocket?.Close();
            _udpSocket?.Close();
        }

        private static void ListenForDevices()
        {
            var buffer = new byte[1024];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                try
                {
                    int received = _udpSocket.ReceiveFrom(buffer, ref remoteEP);
                    var data = buffer.Take(received).ToArray();

                    // Try to deserialize as Measurement first
                    try
                    {
                        var measurement = NetworkHelper.DeserializeObject<Measurement>(data);
                        lock (_measurements)
                        {
                            _measurements.Add(measurement);
                        }
                        Console.WriteLine($"Received measurement from {measurement.DeviceId}: {measurement.Value}{measurement.Unit}");
                    }
                    catch
                    {
                        // If not a measurement, try as Alarm
                        try
                        {
                            var alarm = NetworkHelper.DeserializeObject<Alarm>(data);
                            lock (_activeAlarms)
                            {
                                _activeAlarms.Add(alarm);
                            }
                            Console.WriteLine($"Received alarm: {alarm.Cause}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing data: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Device listening error: {ex.Message}");
                }
            }
        }

        private static void CommunicateWithServer()
        {
            while (true)
            {
                try
                {
                    // Update station data
                    lock (_measurements)
                    {
                        _stationInfo.Measurements = new List<Measurement>(_measurements);
                        _measurements.Clear();
                    }

                    lock (_activeAlarms)
                    {
                        _stationInfo.ActiveAlarms = new List<Alarm>(_activeAlarms);
                        _activeAlarms.Clear();
                    }

                    // Send to server
                    NetworkHelper.SendMessage(_tcpSocket, _stationInfo);
                    Thread.Sleep(1000); // Send update every second
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Server communication error: {ex.Message}");
                    break;
                }
            }
        }
    }
}