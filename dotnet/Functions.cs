using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace TerevintoSoftware.WeatherApi;

public class ImportData(ILogger<ImportData> logger)
{
    private readonly ILogger<ImportData> _logger = logger;

    private static DateTimeOffset? _lastDateRetrieved = null;
    private static List<TimeseriesPoint> _dataRetrieved = [];

    [Function("ImportData")]
    public async Task<MultiResponse> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var body = (await req.ReadFromJsonAsync<SensorInputDto>())!;
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { Status = "OK" });

        _logger.LogInformation("Processed new data point. Temperature: {Temperature}, humidity: {Humidity}", body.Temperature, body.Temperature);

        return new MultiResponse
        {
            HttpResponse = response,
            Document = new SensorData
            {
                id = Guid.NewGuid(),
                dateTime = DateTimeOffset.Now,
                humidity = body.Humidity,
                temperature = body.Temperature
            }
        };
    }

    private static string _index = "";

    [Function("index")]
    public HttpResponseData GetHomePage([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        if (string.IsNullOrEmpty(_index))
        {
#if DEBUG
            _index = File.ReadAllText("index.html");
#else
            _index = File.ReadAllText("C:\\home\\site\\wwwroot\\index.html");
#endif

            _index = _index.Replace("%%REPLACE_TIMESERIES_API%%", Environment.GetEnvironmentVariable("TIMESERIES_API_URL"));
        }

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html");
        response.WriteString(_index);

        return response;
    }

    [Function("Timeseries")]
    public async Task<HttpResponseData> GetTimeseries(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
        [CosmosDBInput("data", "timeseries", Connection = "CosmosDbConnectionSetting")] IEnumerable<SensorData> sensorData)
    {
        var query = sensorData;

        _logger.LogInformation($"Last date retrieved: {_lastDateRetrieved}");

        if (_lastDateRetrieved != null)
        {
            query = sensorData.Where(x => x.dateTime > _lastDateRetrieved);
        }

        var data = (_lastDateRetrieved == null ? sensorData : query)
            .Select(x => new TimeseriesPoint
            {
                Date = x.dateTime,
                Humidity = x.humidity,
                Temperature = x.temperature
            })
            .ToList();

        _logger.LogInformation($"Data points retrieved: {data.Count}");

        if (data.Count > 0)
        {
            _lastDateRetrieved = data.Max(x => x.Date);
        }

        _dataRetrieved = _dataRetrieved.Concat(data).ToList();

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(_dataRetrieved);

        return response;
    }
}
