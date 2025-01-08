using ConsoleTables;
using MeteoNet.Core.Models;

namespace MeteoNet.Station;

public class ConsoleUI
{
    private readonly Dictionary<string, string> _deviceLastMeasurement = new();
    private readonly Queue<(DateTime Timestamp, string Message)> _alarmLog = new();
    private const int MaxAlarmLogItems = 5;

    public void Initialize(string name, int udpPort)
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║                 MeteoNet Station: {name,-31} ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"Listening for devices on UDP port {udpPort}");
        Console.WriteLine();
    }

    public void AddDevice(string deviceId)
    {
        _deviceLastMeasurement[deviceId] = "No measurements yet";
        LogActivity($"Device connected: {deviceId}");
        RefreshDisplay();
    }

    public void LogMeasurement(string deviceId, Measurement measurement)
    {
        _deviceLastMeasurement[deviceId] = $"{measurement.Type}: {measurement.Value}{measurement.Unit}";
        RefreshDisplay();
    }

    public void LogAlarm(Alarm alarm)
    {
        _alarmLog.Enqueue((DateTime.Now, $"🚨 ALARM: {alarm.Type} - {alarm.Cause} ({alarm.Value})"));
        while (_alarmLog.Count > MaxAlarmLogItems)
        {
            _alarmLog.Dequeue();
        }
        RefreshDisplay();
    }

    private void LogActivity(string message)
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        Console.Clear();
        DrawDevicesTable();
        DrawAlarmLog();
    }

    private void DrawDevicesTable()
    {
        var table = new ConsoleTable("Device ID", "Last Measurement");

        foreach (var (deviceId, lastMeasurement) in _deviceLastMeasurement)
        {
            table.AddRow(deviceId, lastMeasurement);
        }

        table.Write();
        Console.WriteLine();
    }

    private void DrawAlarmLog()
    {
        if (_alarmLog.Count == 0)
        {
            return;
        }

        Console.WriteLine("Recent Alarms:");
        Console.WriteLine("──────────────────────────────────────────────────────────────────");

        foreach (var (timestamp, message) in _alarmLog.Reverse())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{timestamp:HH:mm:ss} - {message}");
            Console.ResetColor();
        }
    }
}
