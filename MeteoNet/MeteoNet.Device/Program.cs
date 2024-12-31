using MeteoNet.Core.Enums;
using MeteoNet.Core.Models;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.Clear();
Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                    MeteoNet Device Simulator                   ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");

Console.Write("Enter station IP (default: 127.0.0.1): ");
var ip = Console.ReadLine();
ip = string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;

Console.Write("Enter station UDP port (default: 5001): ");
var portInput = Console.ReadLine();
var port = string.IsNullOrWhiteSpace(portInput) ? 5001 : int.Parse(portInput);

var device = new MeteoNet.Device.UdpClient(ip, port);

try
{
    Console.Clear();
    device.InitializeAsync(
        "Device-1",
        new[] { MeasurementType.Temperature, MeasurementType.Humidity }
    );

    Console.WriteLine($"Connected to station at {ip}:{port}");
    Console.WriteLine("Sending measurements every 5 seconds...");
    Console.WriteLine();

    while (true)
    {
        var measurement = new Measurement
        {
            DeviceId = "Device-1",
            Type = MeasurementType.Temperature,
            Value = Random.Shared.Next(15, 35),
            Unit = "°C",
            Timestamp = DateTime.Now
        };

        device.SendMeasurementAsync(measurement);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sent: {measurement.Type} = {measurement.Value}{measurement.Unit}");

        if (measurement.Value > 30)
        {
            var alarm = new Alarm
            {
                Type = AlarmType.HighTemperature,
                Value = measurement.Value,
                Cause = "Temperature too high",
                Timestamp = DateTime.Now
            };

            device.RaiseAlarmAsync(alarm);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🚨 ALARM: {alarm.Type} ({alarm.Cause})");
            Console.ResetColor();
        }

        Thread.Sleep(5000);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Device error: {ex.Message}");
}
