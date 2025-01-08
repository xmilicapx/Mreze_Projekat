using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using MeteoNet.Core.Models;

namespace MeteoNet.Station;

[Obsolete("Obsolete")]
public class UdpServerService
{
    private readonly UdpClient _server;
    private readonly BinaryFormatter _formatter;
    private readonly int _port;
    private bool _isRunning;
    private readonly Action<Measurement> _onMeasurementReceived;
    private readonly Action<Alarm> _onAlarmReceived;

    public UdpServerService(int port, Action<Measurement> onMeasurementReceived, Action<Alarm> onAlarmReceived)
    {
        _port = port;
        _server = new UdpClient(_port);
        _formatter = new BinaryFormatter();
        _onMeasurementReceived = onMeasurementReceived;
        _onAlarmReceived = onAlarmReceived;
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        Console.WriteLine($"UDP Server started on port {_port}");

        while (_isRunning)
        {
            try
            {
                var result = await _server.ReceiveAsync();
                using var stream = new MemoryStream(result.Buffer);
                var data = _formatter.Deserialize(stream);

                switch (data)
                {
                    case Measurement measurement:
                        _onMeasurementReceived(measurement);
                        break;
                    case Alarm alarm:
                        _onAlarmReceived(alarm);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving UDP data: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _server.Close();
    }
}