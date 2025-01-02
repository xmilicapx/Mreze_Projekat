using MeteoNet.Server;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Enter port number (default: 5000):");
var portInput = Console.ReadLine();
var port = string.IsNullOrWhiteSpace(portInput) ? 5000 : int.Parse(portInput);

var server = new TcpServer(port);

try
{
    await server.StartAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Server error: {ex.Message}");
}