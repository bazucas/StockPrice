using Npgsql; // Import Npgsql for PostgreSQL database interaction
using StockPrice.Api; // Import the main API namespace for the StockPrice app
using StockPrice.Api.Realtime; // Import for real-time stock feed functionality
using StockPrice.Api.Stocks; // Import for handling stock-related services

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (dependency injection).
builder.Services.AddEndpointsApiExplorer(); // Adds support for discovering API endpoints
builder.Services.AddSwaggerGen(); // Adds Swagger generation for API documentation
builder.Services.AddCors(); // Adds CORS support to control allowed origins for requests
builder.Services.AddMemoryCache(); // Adds an in-memory cache service
builder.Services.AddSignalR(); // Adds SignalR for real-time WebSocket communication

// Register a singleton for the PostgreSQL data source
builder.Services.AddSingleton(_ =>
{
    // Retrieve the connection string from the configuration file
    var connectionString = builder.Configuration.GetConnectionString("Database")!;

    // Create an NpgsqlDataSource instance using the connection string
    var npgsqlDataSource = NpgsqlDataSource.Create(connectionString);

    return npgsqlDataSource; // Return the data source for dependency injection
});

// Register the DatabaseInitializer as a hosted service (runs in the background)
builder.Services.AddHostedService<DatabaseInitializer>();

// Register the StocksClient with an HttpClient, setting the base address from configuration
builder.Services.AddHttpClient<StocksClient>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["Stocks:ApiUrl"]!); // Set the base URL for stock API calls
});

// Register StockService as a scoped service (one instance per request)
builder.Services.AddScoped<StockService>();

// Register ActiveTickerManager as a singleton (shared instance across the app)
builder.Services.AddSingleton<ActiveTickerManager>();

// Register StocksFeedUpdater as a hosted service (runs in the background for stock updates)
builder.Services.AddHostedService<StocksFeedUpdater>();

// Configure StockUpdateOptions using settings from the configuration file
builder.Services.Configure<StockUpdateOptions>(builder.Configuration.GetSection("StockUpdateOptions"));

var app = builder.Build(); // Build the web application

// If in development mode, enable Swagger UI and configure CORS policies
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enable Swagger for API documentation
    app.UseSwaggerUI(); // Enable the Swagger UI for interactive API testing

    // Configure CORS to allow specific origins defined in configuration
    app.UseCors(policy => policy
        .WithOrigins(builder.Configuration["Cors:AllowedOrigin"]!) // Allowed origin from config
        .AllowAnyHeader() // Allow all headers
        .AllowAnyMethod() // Allow all HTTP methods (GET, POST, PUT, DELETE, etc.)
        .AllowCredentials()); // Allow sending credentials (cookies, authentication tokens)
}

// Map a GET endpoint for retrieving stock prices by ticker symbol
app.MapGet("/api/stocks/{ticker}", async (string ticker, StockService stockService) =>
{
    // Fetch the latest stock price using the StockService
    var result = await stockService.GetLatestStockPrice(ticker);

    // Return 404 Not Found if no stock data is available, otherwise return 200 OK with the data
    return result is null
        ? Results.NotFound($"No stock data available for ticker: {ticker}")
        : Results.Ok(result);
})
    .WithName("GetLatestStockPrice") // Set the name of the route
    .WithOpenApi(); // Expose the endpoint in the OpenAPI (Swagger) documentation

// Map a SignalR hub for real-time stock updates
app.MapHub<StocksFeedHub>("/stocks-feed");

// Enable HTTPS redirection (forces HTTP requests to use HTTPS)
app.UseHttpsRedirection();

app.Run(); // Run the web application
