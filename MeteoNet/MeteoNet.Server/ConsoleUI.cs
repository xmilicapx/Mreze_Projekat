using ConsoleTables;
using MeteoNet.Core.Models;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics.Metrics;
using System.Security.Claims;

namespace MeteoNet.Server;

public class ConsoleUI
{
    private const int MaxHistoryItems = 10;
    private readonly Queue<(DateTime Timestamp, string Message)> _measurementHistory = new();
    private readonly List<Station> _connectedStations = new();

    public void Initialize()
    {
        Console.Clear();
        DrawHeader();
    }

    public void AddStation(Station station)
    {
        _connectedStations.Add(station);
        RefreshDisplay();
    }

    public void RemoveStation(Station station)
    {
        _connectedStations.Remove(station);
        RefreshDisplay();
    }

    public void AddMeasurement(Station station, Measurement measurement)
    {
        _measurementHistory.Enqueue((
            DateTime.Now,
            $"[{station.Name}] {measurement.Type}: {measurement.Value}{measurement.Unit}"
        ));

        while (_measurementHistory.Count > MaxHistoryItems)
        {
            _measurementHistory.Dequeue();
        }

        RefreshDisplay();
    }

    public void AddAlarm(Station station, Alarm alarm)
    {
        _measurementHistory.Enqueue((
            DateTime.Now,
            $"🚨 [{station.Name}] ALARM: {alarm.Type} - {alarm.Cause} ({alarm.Value})"
        ));

        while (_measurementHistory.Count > MaxHistoryItems)
        {
            _measurementHistory.Dequeue();
        }

        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        Console.Clear();
        DrawHeader();
        DrawStations();
        DrawHistory();
    }

    private void DrawHeader()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    MeteoNet Central Server                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
    }

    private void DrawStations()
    {
        if (_connectedStations.Count == 0)
        {
            Console.WriteLine("No stations connected.");
            Console.WriteLine();
            return;
        }

        var table = new ConsoleTable("Station Name", "Location", "Population", "Devices");

        foreach (var station in _connectedStations)
        {
            table.AddRow(
                station.Name,
                $"{station.Coordinates.Latitude:N4}°N, {station.Coordinates.Longitude:N4}°E",
                station.Population.ToString("N0"),
                station.DeviceCount.ToString()
            );
        }

        table.Write();
        Console.WriteLine();
    }

    private void DrawHistory()
    {
        Console.WriteLine("Recent Activity:");
        Console.WriteLine("──────────────────────────────────────────────────────────────────");

        if (_measurementHistory.Count == 0)
        {
            Console.WriteLine("No activity yet.");
            return;
        }

        foreach (var (timestamp, message) in _measurementHistory.Reverse())
        {
            if (message.Contains("ALARM"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{timestamp:HH:mm:ss} - {message}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"{timestamp:HH:mm:ss} - {message}");
            }
        }
    }
}