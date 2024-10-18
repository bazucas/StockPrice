using Newtonsoft.Json;

namespace StockPrice.Api.Stocks;

public class AlphaVantageData
{
    [JsonProperty("Meta Data")]
    public required MetaData MetaData { get; set; }
    [JsonProperty("Time Series (15min)")]
    public required Dictionary<string, TimeSeriesEntry> TimeSeries { get; set; }
}

public class MetaData
{
    [JsonProperty("1. Information")]
    public required string Information { get; set; }
    [JsonProperty("2. Symbol")]
    public required string Symbol { get; set; }
    [JsonProperty("3. Last Refreshed")]
    public required string LastRefreshed { get; set; }
    [JsonProperty("4. Interval")]
    public required string Interval { get; set; }
    [JsonProperty("5. Output Size")]
    public required string OutputSize { get; set; }
    [JsonProperty("6. Time Zone")]
    public required string TimeZone { get; set; }
}

public class TimeSeriesEntry
{
    [JsonProperty("1. open")]
    public required string Open { get; set; }
    [JsonProperty("2. high")]
    public required string High { get; set; }
    [JsonProperty("3. low")]
    public required string Low { get; set; }
    [JsonProperty("4. close")]
    public required string Close { get; set; }
    [JsonProperty("5. volume")]
    public required string Volume { get; set; }
}
