using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using MeteoNet.Core.Models;
using ConsoleTables;

namespace MeteoNet.Server;

public class TcpServer
{
    private readonly TcpListener _listener;
    private readonly int _port;
    private Station? _connectedStation;
    private bool _isRunning;
    private readonly ConsoleUI _ui = new();

    public TcpServer(int port = 5000)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, _port);
    }

    [Obsolete("Obsolete")]
    public async Task StartAsync()
    {
        _listener.Start();
        _isRunning = true;
        _ui.Initialize();
        Console.WriteLine($"Server started on port {_port}");

        while (_isRunning)
        {
            if (_connectedStation == null)
            {
                var client = await _listener.AcceptTcpClientAsync();
                await HandleClientConnectionAsync(client);
            }

            await Task.Delay(100);
        }
    }

    [Obsolete("Obsolete")]
    private async Task HandleClientConnectionAsync(TcpClient client)
    {
        Console.WriteLine("New station connected!");

        try
        {
            await using var stream = client.GetStream();
            var formatter = new BinaryFormatter();

            while (client.Connected)
            {
                if (stream.DataAvailable)
                {
                    var data = formatter.Deserialize(stream);

                    switch (data)
                    {
                        case Station station:
                            HandleStationRegistration(station);
                            break;
                        case Measurement measurement:
                            HandleMeasurement(measurement);
                            break;
                        case Alarm alarm:
                            HandleAlarm(alarm);
                            break;
                    }
                }

                await Task.Delay(100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}");
        }
        finally
        {
            _connectedStation = null;
            client.Close();
        }
    }

    private void HandleStationRegistration(Station station)
    {
        _connectedStation = station;
        _ui.AddStation(station);
    }

    private void HandleMeasurement(Measurement measurement)
    {
        if (_connectedStation == null) return;

        _connectedStation.Measurements.Add(measurement);
        _ui.AddMeasurement(_connectedStation, measurement);
    }

    private void HandleAlarm(Alarm alarm)
    {
        if (_connectedStation == null) return;

        _connectedStation.ActiveAlarms.Add(alarm);
        _ui.AddAlarm(_connectedStation, alarm);
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();
    }
}