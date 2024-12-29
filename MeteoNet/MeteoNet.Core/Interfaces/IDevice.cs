using MeteoNet.Core.Enums;
using MeteoNet.Core.Models;
using System.Diagnostics.Metrics;

namespace MeteoNet.Core.Interfaces;

public interface IDevice
{
    void InitializeAsync(string deviceId, MeasurementType[] supportedMeasurements);
    void SendMeasurementAsync(Measurement measurement);
    void RaiseAlarmAsync(Alarm alarm);
}