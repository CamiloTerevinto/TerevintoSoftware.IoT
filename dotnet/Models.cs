using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace TerevintoSoftware.WeatherApi;

public class SensorInputDto
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
}

public class TimeseriesPoint
{
    public DateTimeOffset Date { get; set; }
    public double? Humidity { get; set; }
    public double? Temperature { get; set; }
}

public class MultiResponse
{
    [CosmosDBOutput("data", "timeseries", Connection = "CosmosDbConnectionSetting")]
    public required SensorData Document { get; set; }
    public required HttpResponseData HttpResponse { get; set; }
}

public class SensorData
{
    public required Guid id { get; set; }
    public required DateTimeOffset dateTime { get; set; }
    public required double temperature { get; set; }
    public required double humidity { get; set; }
}