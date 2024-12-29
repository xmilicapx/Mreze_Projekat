namespace MeteoNet.Core.Models;

[Serializable]
public class Station
{
    public string Name { get; set; } = string.Empty;          // Geographical location
    public Coordinates Coordinates { get; set; } = null!;
    public int Population { get; set; }
    public int DeviceCount { get; set; }
    public List<Measurement> Measurements { get; set; } = new();
    public List<Alarm> ActiveAlarms { get; set; } = new();
}