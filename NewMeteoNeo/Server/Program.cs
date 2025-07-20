using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using Core.Models;

namespace Server
{
    internal class Program
    {
        private const int SERVER_PORT = 10000;
        private static Socket _tcpSocket;
        private static List<Socket> _stationSockets = new List<Socket>();
        private static Dictionary<Socket, Station> _stations = new Dictionary<Socket, Station>();

        public static void Main(string[] args)
        {
            InitializeServer();
            StartServer();
        }

        private static void InitializeServer()
        {
            try
            {
                _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _tcpSocket.Bind(new IPEndPoint(IPAddress.Any, SERVER_PORT));
                _tcpSocket.Listen(10);
                Console.WriteLine($"Server started on port {SERVER_PORT}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server initialization error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static void StartServer()
        {
            // Start station listener thread
            var listenerThread = new Thread(AcceptStations);
            listenerThread.Start();

            // Start data display thread
            var displayThread = new Thread(DisplayData);
            displayThread.Start();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();

            foreach (var socket in _stationSockets)
            {
                socket?.Close();
            }
            _tcpSocket?.Close();
        }

        private static void AcceptStations()
        {
            while (true)
            {
                try
                {
                    Socket stationSocket = _tcpSocket.Accept();

                    // Create new station info
                    var station = new Station
                    {
                        Name = $"Station_{_stationSockets.Count + 1}",
                        Coordinates = new Coordinates(45.0 + _stationSockets.Count, 20.0),
                        Population = 100000,
                        DeviceCount = 5
                    };

                    // Send initial station info
                    NetworkHelper.SendMessage(stationSocket, station);

                    lock (_stations)
                    {
                        _stationSockets.Add(stationSocket);
                        _stations.Add(stationSocket, station);
                    }

                    // Start receiving data from this station
                    var stationThread = new Thread(() => ReceiveStationData(stationSocket));
                    stationThread.Start();

                    Console.WriteLine($"New station connected: {station.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting station: {ex.Message}");
                }
            }
        }

        private static void ReceiveStationData(Socket stationSocket)
        {
            while (true)
            {
                try
                {
                    var updatedStation = NetworkHelper.ReceiveMessage<Station>(stationSocket);

                    lock (_stations)
                    {
                        if (_stations.ContainsKey(stationSocket))
                        {
                            _stations[stationSocket] = updatedStation;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving station data: {ex.Message}");
                    break;
                }
            }

            // Clean up disconnected station
            lock (_stations)
            {
                _stations.Remove(stationSocket);
                _stationSockets.Remove(stationSocket);
            }
            stationSocket.Close();
        }

        private static void DisplayData()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n=== METEOROLOGICAL STATION NETWORK STATUS ===\n");
                Console.WriteLine($"Active Stations: {_stations.Count}\n");

                lock (_stations)
                {
                    foreach (var station in _stations.Values)
                    {
                        Console.WriteLine($"=== {station.Name} ===");
                        Console.WriteLine($"Location: {station.Coordinates.Latitude:F2}°N, {station.Coordinates.Longitude:F2}°E");
                        Console.WriteLine($"Population: {station.Population:N0}");
                        Console.WriteLine("Recent Measurements:");

                        foreach (var measurement in station.Measurements)
                        {
                            Console.WriteLine($"- {measurement.DeviceId}: {measurement.Value}{measurement.Unit}");
                        }

                        if (station.ActiveAlarms.Any())
                        {
                            Console.WriteLine("\nACTIVE ALARMS:");
                            foreach (var alarm in station.ActiveAlarms)
                            {
                                Console.WriteLine($"!!! {alarm.Cause} !!!");
                            }
                        }
                        Console.WriteLine();
                    }
                }

                Thread.Sleep(1000); // Update display every second
            }
        }
    }
}