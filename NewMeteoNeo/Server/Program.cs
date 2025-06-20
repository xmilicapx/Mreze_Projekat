using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core;
using Core.Models;
using static System.Collections.Specialized.BitVector32;

namespace Server
{
    internal class Program
    {
        private const int Port = 51000;
        private const int MaxClients = 10;
        private static List<Socket> _clientSockets;
        private static Socket _listenSocket;

        public static void Main(string[] args)
        {
            _clientSockets = new List<Socket>();
            StartServer();
        }

        private static void StartServer()
        {
            try
            {
                // Create TCP socket as per guide
                _listenSocket = new Socket(
                    AddressFamily.InterNetwork,  // IPv4
                    SocketType.Stream,           // TCP
                    ProtocolType.Tcp
                );

                // Bind to local endpoint
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);
                _listenSocket.Bind(endPoint);

                // Start listening with backlog
                _listenSocket.Listen(MaxClients);
                Console.WriteLine($"Server started on port {Port}");

                while (true)
                {
                    // Accept new connections
                    var clientSocket = _listenSocket.Accept();
                    _clientSockets.Add(clientSocket);
                    Console.WriteLine($"Client connected: {clientSocket.RemoteEndPoint}");

                    var station = new Station
                    {
                        Name = "Belgrade Station",
                        Coordinates = new Coordinates(44.787197, 20.457273),
                        Population = 1_700_000,
                        DeviceCount = 3
                    };

                    // Send initialization data
                    NetworkHelper.SendMessage(clientSocket, station);

                    // Start handling client data in a separate thread
                    var clientThread = new Thread(() => HandleClient(clientSocket));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
                if (_listenSocket != null)
                {
                    _listenSocket.Close();
                }
            }
        }

        private static void ProcessClientData(Socket clientSocket)
        {
            try
            {
                var buffer = new byte[4096]; // Larger buffer for measurement lists
                var received = clientSocket.Receive(buffer);

                if (received > 0)
                {
                    var measurements = NetworkHelper.DeserializeObject<List<Measurement>>(buffer);

                    // Process and display measurements
                    Console.WriteLine($"\nReceived {measurements.Count} measurements from station:");
                    foreach (var measurement in measurements)
                    {
                        Console.WriteLine($"  {measurement.Type}: {measurement.Value:F2} {measurement.Unit} " +
                                        $"(Device: {measurement.DeviceId}, Time: {measurement.Timestamp:HH:mm:ss})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing client data: {ex.Message}");
            }
        }

        private static void HandleClient(Socket clientSocket)
        {
            while (true)
            {
                try
                {
                    if (clientSocket.Poll(1000000, SelectMode.SelectRead)) // 1 second timeout
                    {
                        ProcessClientData(clientSocket);
                    }
                    Thread.Sleep(1000); // Wait 1 second before next check
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client: {ex.Message}");
                    break;
                }
            }

            clientSocket.Close();
            Console.WriteLine("Client disconnected");
        }
    }
}