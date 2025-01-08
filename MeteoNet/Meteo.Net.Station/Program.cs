using MeteoNet.Station;
using MeteoNet.Core.Models;
using MeteoNet.Station;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Enter server IP (default: 127.0.0.1):");
var ip = Console.ReadLine();
ip = string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;

Console.WriteLine("Enter server port (default: 5000):");
var portInput = Console.ReadLine();
var port = string.IsNullOrWhiteSpace(portInput) ? 5000 : int.Parse(portInput);

var station = new TcpClient(ip, port);

try
{
    var stationName = "Test Station";

    // First initialize the TCP connection
    station.InitializeAsync(
        stationName,
        new Coordinates(44.787197, 20.457273),
        1000000,
        5
    );

    // Then start the UDP server for devices
    var deviceManager = new DeviceManager(station);
    await deviceManager.StartAsync(stationName);

    // Keep the application running
    Console.WriteLine("Press Enter to exit...");
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Station error: {ex.Message}");
}
