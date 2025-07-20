using System;
using System.Collections.Generic;
using Core.Models;
using Core.Enums;

namespace Device
{
    public static class MeasurementGenerator
    {
        private static readonly Random _random = new Random();

        public static Measurement GenerateMeasurement(MeasurementType type, string deviceId)
        {
            switch (type)
            {
                case MeasurementType.Temperature:
                    return GenerateTemperatureMeasurement(deviceId);
                case MeasurementType.Humidity:
                    return GenerateHumidityMeasurement(deviceId);
                case MeasurementType.WindSpeed:
                    return GenerateWindSpeedMeasurement(deviceId);
                case MeasurementType.WindDirection:
                    return GenerateWindDirectionMeasurement(deviceId);
                case MeasurementType.Pressure:
                    return GeneratePressureMeasurement(deviceId);
                case MeasurementType.Precipitation:
                    return GeneratePrecipitationMeasurement(deviceId);
                case MeasurementType.ChemicalComposition:
                    return GenerateChemicalCompositionMeasurement(deviceId);
                case MeasurementType.Cloudiness:
                    return GenerateCloudinessMeasurement(deviceId);
                default:
                    throw new ArgumentException($"Unsupported measurement type: {type}");
            }
        }

        public static List<Alarm> CheckForAlarms(Measurement measurement)
        {
            var alarms = new List<Alarm>();

            switch (measurement.Type)
            {
                case MeasurementType.Temperature:
                    alarms.AddRange(CheckTemperatureAlarms(measurement));
                    break;
                case MeasurementType.Humidity:
                    alarms.AddRange(CheckHumidityAlarms(measurement));
                    break;
                case MeasurementType.WindSpeed:
                    alarms.AddRange(CheckWindSpeedAlarms(measurement));
                    break;
                case MeasurementType.WindDirection:
                    alarms.AddRange(CheckWindDirectionAlarms(measurement));
                    break;
                case MeasurementType.Pressure:
                    alarms.AddRange(CheckPressureAlarms(measurement));
                    break;
                case MeasurementType.Precipitation:
                    alarms.AddRange(CheckPrecipitationAlarms(measurement));
                    break;
                case MeasurementType.ChemicalComposition:
                    alarms.AddRange(CheckChemicalCompositionAlarms(measurement));
                    break;
                case MeasurementType.Cloudiness:
                    alarms.AddRange(CheckCloudinessAlarms(measurement));
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported measurement type: {measurement.Type}");
            }

            return alarms;
        }

        // Temperature methods
        private static Measurement GenerateTemperatureMeasurement(string deviceId)
        {
            return new Measurement
            {
                DeviceId = deviceId,
                Type = MeasurementType.Temperature,
                Value = _random.Next(-10, 40),
                Unit = "°C",
                Timestamp = DateTime.Now
            };
        }

        private static List<Alarm> CheckTemperatureAlarms(Measurement measurement)
        {
            var alarms = new List<Alarm>();

            if (measurement.Value > 35)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.HighTemperature,
                    Value = measurement.Value,
                    Cause = $"High temperature detected: {measurement.Value}°C",
                    Timestamp = DateTime.Now
                });
            }
            else if (measurement.Value < -5)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.LowTemperature,
                    Value = measurement.Value,
                    Cause = $"Low temperature detected: {measurement.Value}°C",
                    Timestamp = DateTime.Now
                });
            }

            return alarms;
        }

        // Humidity methods
        private static Measurement GenerateHumidityMeasurement(string deviceId)
        {
            return new Measurement
            {
                DeviceId = deviceId,
                Type = MeasurementType.Humidity,
                Value = _random.Next(0, 101),
                Unit = "%",
                Timestamp = DateTime.Now
            };
        }

        private static List<Alarm> CheckHumidityAlarms(Measurement measurement)
        {
            var alarms = new List<Alarm>();

            if (measurement.Value > 90)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.HighHumidity,
                    Value = measurement.Value,
                    Cause = $"High humidity detected: {measurement.Value}%",
                    Timestamp = DateTime.Now
                });
            }
            else if (measurement.Value < 10)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.LowHumidity,
                    Value = measurement.Value,
                    Cause = $"Low humidity detected: {measurement.Value}%",
                    Timestamp = DateTime.Now
                });
            }

            return alarms;
        }

        // Wind Speed methods
        private static Measurement GenerateWindSpeedMeasurement(string deviceId)
        {
            return new Measurement
            {
                DeviceId = deviceId,
                Type = MeasurementType.WindSpeed,
                Value = _random.Next(0, 120),
                Unit = "km/h",
                Timestamp = DateTime.Now
            };
        }

        private static List<Alarm> CheckWindSpeedAlarms(Measurement measurement)
        {
            var alarms = new List<Alarm>();

            if (measurement.Value > 80)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.HighWindSpeed,
                    Value = measurement.Value,
                    Cause = $"High wind speed detected: {measurement.Value} km/h",
                    Timestamp = DateTime.Now
                });
            }

            return alarms;
        }

        // Wind Direction methods
        private static Measurement GenerateWindDirectionMeasurement(string deviceId)
        {
            return new Measurement
            {
                DeviceId = deviceId,
                Type = MeasurementType.WindDirection,
                Value = _random.Next(0, 360),
                Unit = "°",
                Timestamp = DateTime.Now
            };
        }

        private static List<Alarm> CheckWindDirectionAlarms(Measurement measurement)
        {
            return new List<Alarm>();
        }

        // Pressure methods
        private static Measurement GeneratePressureMeasurement(string deviceId)
        {
            return new Measurement
            {
                DeviceId = deviceId,
                Type = MeasurementType.Pressure,
                Value = _random.Next(950, 1051),
                Unit = "hPa",
                Timestamp = DateTime.Now
            };
        }

        private static List<Alarm> CheckPressureAlarms(Measurement measurement)
        {
            var alarms = new List<Alarm>();

            if (measurement.Value > 1040)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.HighPressure,
                    Value = measurement.Value,
                    Cause = $"High pressure detected: {measurement.Value} hPa",
                    Timestamp = DateTime.Now
                });
            }
            else if (measurement.Value < 960)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.LowPressure,
                    Value = measurement.Value,
                    Cause = $"Low pressure detected: {measurement.Value} hPa",
                    Timestamp = DateTime.Now
                });
            }

            return alarms;
        }

        // Precipitation methods
        private static Measurement GeneratePrecipitationMeasurement(string deviceId)
        {
            return new Measurement
            {
                DeviceId = deviceId,
                Type = MeasurementType.Precipitation,
                Value = _random.Next(0, 51),
                Unit = "mm/h",
                Timestamp = DateTime.Now
            };
        }

        private static List<Alarm> CheckPrecipitationAlarms(Measurement measurement)
        {
            var alarms = new List<Alarm>();

            if (measurement.Value > 30)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.ExcessivePrecipitation,
                    Value = measurement.Value,
                    Cause = $"Excessive precipitation detected: {measurement.Value} mm/h",
                    Timestamp = DateTime.Now
                });
            }

            return alarms;
        }

        // Chemical Composition methods
        private static Measurement GenerateChemicalCompositionMeasurement(string deviceId)
        {
            return new Measurement
            {
                DeviceId = deviceId,
                Type = MeasurementType.ChemicalComposition,
                Value = _random.Next(0, 101),
                Unit = "ppm",
                Timestamp = DateTime.Now
            };
        }

        private static List<Alarm> CheckChemicalCompositionAlarms(Measurement measurement)
        {
            var alarms = new List<Alarm>();

            if (measurement.Value > 80)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.HighChemicalConcentration,
                    Value = measurement.Value,
                    Cause = $"High chemical concentration detected: {measurement.Value} ppm",
                    Timestamp = DateTime.Now
                });
            }
            else if (measurement.Value < 5)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.LowChemicalConcentration,
                    Value = measurement.Value,
                    Cause = $"Low chemical concentration detected: {measurement.Value} ppm",
                    Timestamp = DateTime.Now
                });
            }

            return alarms;
        }

        // Cloudiness methods
        private static Measurement GenerateCloudinessMeasurement(string deviceId)
        {
            return new Measurement
            {
                DeviceId = deviceId,
                Type = MeasurementType.Cloudiness,
                Value = _random.Next(0, 101),
                Unit = "%",
                Timestamp = DateTime.Now
            };
        }

        private static List<Alarm> CheckCloudinessAlarms(Measurement measurement)
        {
            var alarms = new List<Alarm>();

            if (measurement.Value > 95)
            {
                alarms.Add(new Alarm
                {
                    Type = AlarmType.HighCloudiness,
                    Value = measurement.Value,
                    Cause = $"High cloudiness detected: {measurement.Value}%",
                    Timestamp = DateTime.Now
                });
            }

            return alarms;
        }
    }
}
