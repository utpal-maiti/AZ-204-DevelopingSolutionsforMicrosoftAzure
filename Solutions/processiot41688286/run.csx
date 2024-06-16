#r "Newtonsoft.Json"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public static void Run(string deviceReading, out object outputDocument, TraceWriter log)
{
    dynamic reading = JObject.Parse(deviceReading);
    outputDocument = new
    {
        deviceid = reading.DeviceID,
        time = reading.Time,
        reading = reading.Reading
    };
    log.Info($"Processed a reading for {reading.DeviceID} from {reading.Time}");
}