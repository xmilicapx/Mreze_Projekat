using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using MeteoNet.Core.Enums;
using MeteoNet.Core.Interfaces;
using MeteoNet.Core.Models;

namespace MeteoNet.Device;

[Obsolete("Obsolete")]
public class UdpClient : IDevice
{
    private readonly System.Net.Sockets.UdpClient _client;
    private readonly string _stationIp;
    private readonly int _stationPort;
    private readonly BinaryFormatter _formatter;
    private string _deviceId = string.Empty;
    private MeasurementType[] _supportedMeasurements = Array.Empty<MeasurementType>();

    public UdpClient(string stationIp = "127.0.0.1", int stationPort = 5001)
    {
        _stationIp = stationIp;
        _stationPort = stationPort;
        _client = new System.Net.Sockets.UdpClient();
        _formatter = new BinaryFormatter();
    }

    public void InitializeAsync(string deviceId, MeasurementType[] supportedMeasurements)
    {
        _deviceId = deviceId;
        _supportedMeasurements = supportedMeasurements;
        Console.WriteLine($"Device {deviceId} initialized");
    }

    public void SendMeasurementAsync(Measurement measurement)
    {
        using var stream = new MemoryStream();
        _formatter.Serialize(stream, measurement);
        var data = stream.ToArray();
        _client.Send(data, data.Length, _stationIp, _stationPort);
    }

    public void RaiseAlarmAsync(Alarm alarm)
    {
        using var stream = new MemoryStream();
        _formatter.Serialize(stream, alarm);
        var data = stream.ToArray();
        _client.Send(data, data.Length, _stationIp, _stationPort);
    }
}