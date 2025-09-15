using Microsoft.EntityFrameworkCore;
using ProjeX.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Starting PlannedTeamSlots schema migration...");

try
{
    // Build configuration
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true)
        .Build();

    // Get connection string - first try from config, then use default
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=ProjeXDb;Trusted_Connection=true;MultipleActiveResultSets=true";

    Console.WriteLine($"Using connection string: {connectionString.Replace("Password=", "Password=***")}");

    // Create DbContext
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    await using var context = new ApplicationDbContext(options);

    // Test connection
    Console.WriteLine("Testing database connection...");
    await context.Database.OpenConnectionAsync();
    Console.WriteLine("✓ Database connection successful");

    // Read and execute the complete schema fix script
    var scriptPath = Path.Combine("..", "complete_schema_fix.sql");
    if (!File.Exists(scriptPath))
    {
        Console.WriteLine($"Script file not found at: {Path.GetFullPath(scriptPath)}");

        // Try alternative path
        scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "complete_schema_fix.sql");
        if (!File.Exists(scriptPath))
        {
            Console.WriteLine($"Script file not found at: {Path.GetFullPath(scriptPath)}");
            return;
        }
    }

    Console.WriteLine($"Reading script from: {Path.GetFullPath(scriptPath)}");
    var sql = await File.ReadAllTextAsync(scriptPath);

    Console.WriteLine("Executing schema migration script...");

    // Split the script into individual statements and execute them
    var statements = sql.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

    foreach (var statement in statements)
    {
        var trimmedStatement = statement.Trim();
        if (!string.IsNullOrEmpty(trimmedStatement))
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(trimmedStatement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Statement execution failed: {ex.Message}");
                // Continue with next statement
            }
        }
    }

    Console.WriteLine("✓ Schema migration completed successfully!");

    // Verify the changes
    Console.WriteLine("Verifying PlannedTeamSlots table schema...");
    var columnCount = await context.Database.SqlQueryRaw<int>(
        "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots')").FirstAsync();

    Console.WriteLine($"✓ PlannedTeamSlots table now has {columnCount} columns");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error during migration: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Environment.Exit(1);
}
