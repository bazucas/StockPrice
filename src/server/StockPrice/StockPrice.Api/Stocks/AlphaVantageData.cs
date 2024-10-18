using Newtonsoft.Json; // Used for JSON serialization and deserialization

namespace StockPrice.Api.Stocks;

/// <summary>
/// Represents the data structure for the Alpha Vantage API response, specifically for intraday stock data.
/// Contains metadata and time series entries.
/// </summary>
public class AlphaVantageData
{
    /// <summary>
    /// MetaData contains general information about the API response (symbol, last refreshed, etc.).
    /// </summary>
    [JsonProperty("Meta Data")]
    public required MetaData MetaData { get; set; }

    /// <summary>
    /// TimeSeries stores a dictionary of time series data with the time as the key and the stock prices (open, high, low, close, volume) as the values.
    /// The key is a string representing the timestamp in 15-minute intervals.
    /// </summary>
    [JsonProperty("Time Series (15min)")]
    public required Dictionary<string, TimeSeriesEntry> TimeSeries { get; set; }
}

/// <summary>
/// Contains metadata about the stock data request, such as the symbol, last refreshed time, and time zone.
/// </summary>
public class MetaData
{
    /// <summary>
    /// Describes the purpose of the data request (e.g., "Intraday Prices").
    /// </summary>
    [JsonProperty("1. Information")]
    public required string Information { get; set; }

    /// <summary>
    /// The stock ticker symbol (e.g., MSFT for Microsoft).
    /// </summary>
    [JsonProperty("2. Symbol")]
    public required string Symbol { get; set; }

    /// <summary>
    /// The last time the data was refreshed, typically in UTC.
    /// </summary>
    [JsonProperty("3. Last Refreshed")]
    public required string LastRefreshed { get; set; }

    /// <summary>
    /// The interval of the data (e.g., 15 minutes).
    /// </summary>
    [JsonProperty("4. Interval")]
    public required string Interval { get; set; }

    /// <summary>
    /// The output size of the data (e.g., full or compact).
    /// </summary>
    [JsonProperty("5. Output Size")]
    public required string OutputSize { get; set; }

    /// <summary>
    /// The time zone in which the data is represented (e.g., "US/Eastern").
    /// </summary>
    [JsonProperty("6. Time Zone")]
    public required string TimeZone { get; set; }
}

/// <summary>
/// Represents a single time series entry, containing stock prices for a specific timestamp.
/// </summary>
public class TimeSeriesEntry
{
    /// <summary>
    /// The opening price of the stock for the given time period.
    /// </summary>
    [JsonProperty("1. open")]
    public required string Open { get; set; }

    /// <summary>
    /// The highest price of the stock during the given time period.
    /// </summary>
    [JsonProperty("2. high")]
    public required string High { get; set; }

    /// <summary>
    /// The lowest price of the stock during the given time period.
    /// </summary>
    [JsonProperty("3. low")]
    public required string Low { get; set; }

    /// <summary>
    /// The closing price of the stock for the given time period.
    /// </summary>
    [JsonProperty("4. close")]
    public required string Close { get; set; }

    /// <summary>
    /// The trading volume for the stock during the given time period.
    /// </summary>
    [JsonProperty("5. volume")]
    public required string Volume { get; set; }
}
