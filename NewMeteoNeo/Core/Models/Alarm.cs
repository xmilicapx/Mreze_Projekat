using System;
using Core.Enums;

namespace Core.Models
{
    [Serializable]
    public class Alarm
    {
        public AlarmType Type { get; set; }
        public double Value { get; set; }
        public string Cause { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}