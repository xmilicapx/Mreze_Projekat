using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using MeteoNet.Core.Interfaces;
using MeteoNet.Core.Models;

namespace MeteoNet.Station;

[Obsolete("Obsolete")]
public class TcpClient : IStation
{
    private readonly System.Net.Sockets.TcpClient _client;
    private readonly string _serverIp;
    private readonly int _serverPort;
    private NetworkStream? _stream;
    private readonly BinaryFormatter _formatter;
    private Core.Models.Station _stationInfo = new();

    public TcpClient(string serverIp = "127.0.0.1", int serverPort = 5000)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
        _client = new System.Net.Sockets.TcpClient();
        _formatter = new BinaryFormatter();
    }

    public void InitializeAsync(string name, Coordinates coordinates, int population, int deviceCount)
    {
        try
        {
            _client.ConnectAsync(_serverIp, _serverPort);
            _stream = _client.GetStream();

            _stationInfo = new Core.Models.Station
            {
                Name = name,
                Coordinates = coordinates,
                Population = population,
                DeviceCount = deviceCount
            };

            _formatter.Serialize(_stream, _stationInfo);

            Console.WriteLine($"Station {name} initialized and connected to server");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize station: {ex.Message}");
            throw;
        }
    }

    public void SendMeasurementAsync(Measurement measurement)
    {
        if (_stream == null) throw new InvalidOperationException("Station not initialized");

        _formatter.Serialize(_stream, measurement);

        _stationInfo.Measurements.Add(measurement);
    }

    public void RaiseAlarmAsync(Alarm alarm)
    {
        if (_stream == null) throw new InvalidOperationException("Station not initialized");

        _formatter.Serialize(_stream, alarm);

        _stationInfo.ActiveAlarms.Add(alarm);
    }

    public IEnumerable<Measurement> GetMeasurementsAsync()
    {
        return _stationInfo.Measurements.AsEnumerable();
    }

    public IEnumerable<Alarm> GetActiveAlarmsAsync()
    {
        return _stationInfo.ActiveAlarms.AsEnumerable();
    }
}