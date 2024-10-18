using Dapper; // Import Dapper for lightweight ORM functionality (to execute SQL queries)
using Npgsql; // Import Npgsql for PostgreSQL database interaction

namespace StockPrice.Api;

// DatabaseInitializer is a background service that ensures the database and tables are created when the application starts.
internal sealed class DatabaseInitializer(
    NpgsqlDataSource dataSource,    // Npgsql data source for database connection management
    IConfiguration configuration,   // Configuration to access connection strings and other settings
    ILogger<DatabaseInitializer> logger // Logger to log information and errors
) : BackgroundService // Inherits from BackgroundService, meaning it runs in the background
{
    /// <summary>
    /// Main execution method of the background service. It is triggered when the service starts.
    /// This method ensures that the database and required tables are created.
    /// </summary>
    /// <param name="stoppingToken">Token to signal that the service should stop</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Starting database initialization."); // Log that database initialization has started

            // Ensure the database exists; if not, create it
            await EnsureDatabaseExists();

            // Initialize the database schema (create tables and indexes if they do not exist)
            await InitializeDatabase();

            logger.LogInformation("Database initialization completed successfully."); // Log successful completion
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database."); // Log any errors that occur during initialization
        }
    }

    /// <summary>
    /// Ensures that the target database exists by checking its existence in the PostgreSQL system catalog.
    /// If the database doesn't exist, it creates a new one.
    /// </summary>
    private async Task EnsureDatabaseExists()
    {
        // Get the connection string from the configuration
        var connectionString = configuration.GetConnectionString("Database")!;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database; // Extract the database name from the connection string
        builder.Database = "postgres"; // Switch to the default 'postgres' database to check/create the target database

        await using var connection = new NpgsqlConnection(builder.ToString()); // Create a connection to the 'postgres' database
        await connection.OpenAsync(); // Open the connection

        // Check if the database exists by querying the PostgreSQL system catalog (pg_database)
        var databaseExists = await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @databaseName)", // Check for the existence of the database
            new { databaseName });

        // If the database does not exist, create it
        if (!databaseExists)
        {
            logger.LogInformation("Creating database {DatabaseName}", databaseName); // Log the database creation event
            await connection.ExecuteAsync($"CREATE DATABASE {databaseName}"); // Execute the SQL command to create the database
        }
    }

    /// <summary>
    /// Initializes the database schema by creating the 'stock_prices' table and its indexes if they do not already exist.
    /// </summary>
    private async Task InitializeDatabase()
    {
        // SQL script that creates the 'stock_prices' table and necessary indexes
        const string sql =
            """
            -- Check if the table exists, if not, create it
            CREATE TABLE IF NOT EXISTS public.stock_prices (
                id SERIAL PRIMARY KEY,                 -- Auto-incremented primary key
                ticker VARCHAR(10) NOT NULL,           -- Ticker symbol of the stock (e.g., MSFT, AAPL)
                price NUMERIC(12, 6) NOT NULL,         -- Stock price with up to 12 digits and 6 decimals
                timestamp TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC') -- Timestamp with UTC default
            );

            -- Create an index on the ticker column for faster lookups by ticker
            CREATE INDEX IF NOT EXISTS idx_stock_prices_ticker ON public.stock_prices(ticker);

            -- Create an index on the timestamp column for faster queries based on time
            CREATE INDEX IF NOT EXISTS idx_stock_prices_timestamp ON public.stock_prices(timestamp);
            """;

        await using var connection = await dataSource.OpenConnectionAsync(); // Open a connection to the target database
        await connection.ExecuteAsync(sql); // Execute the SQL script to initialize the database schema
    }
}
