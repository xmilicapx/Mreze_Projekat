using MeteoNet.Station;
using MeteoNet.Core.Models;

namespace MeteoNet.Station;

public class DeviceManager
{
    private readonly TcpClient _tcpClient;
    private readonly UdpServerService _udpServer;
    private readonly int _udpPort;
    private readonly ConsoleUI _ui;

    public DeviceManager(TcpClient tcpClient, int udpPort = 5001)
    {
        _tcpClient = tcpClient;
        _udpPort = udpPort;
        _ui = new ConsoleUI();
        _udpServer = new UdpServerService(
            udpPort,
            measurement => {
                _tcpClient.SendMeasurementAsync(measurement);
                _ui.LogMeasurement(measurement.DeviceId, measurement);
            },
            alarm => {
                _tcpClient.RaiseAlarmAsync(alarm);
                _ui.LogAlarm(alarm);
            }
        );
    }

    public async Task StartAsync(string stationName)
    {
        _ui.Initialize(stationName, _udpPort);
        await _udpServer.StartAsync();
    }

    public void Stop()
    {
        _udpServer.Stop();
    }

    public int Port => _udpPort;
}