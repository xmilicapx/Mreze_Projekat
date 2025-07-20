using System;
using Core.Enums;

namespace Core.Models
{
    [Serializable]
    public class Measurement
    {
        public string DeviceId { get; set; } = string.Empty;
        public MeasurementType Type { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}