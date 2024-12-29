using MeteoNet.Core.Models;
using System.Diagnostics.Metrics;

namespace MeteoNet.Core.Interfaces;

public interface IStation
{
    void InitializeAsync(string name, Coordinates coordinates, int population, int deviceCount);
    void SendMeasurementAsync(Measurement measurement);
    void RaiseAlarmAsync(Alarm alarm);
    IEnumerable<Measurement> GetMeasurementsAsync();
    IEnumerable<Alarm> GetActiveAlarmsAsync();
}