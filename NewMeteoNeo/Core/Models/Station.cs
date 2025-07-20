using System;
using System.Collections.Generic;

namespace Core.Models
{
    [Serializable]
    public class Station
    {
        public string Name { get; set; } = string.Empty;
        public Coordinates Coordinates { get; set; }
        public int Population { get; set; }
        public int DeviceCount { get; set; }
        public List<Measurement> Measurements { get; set; } = new List<Measurement>();
        public List<Alarm> ActiveAlarms { get; set; } = new List<Alarm>();
    }
}