using Microsoft.EntityFrameworkCore;
using ProjeX.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=ProjeXDb;Trusted_Connection=true;MultipleActiveResultSets=true";

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var context = new ApplicationDbContext(options);

try
{
    Console.WriteLine("Starting schema migration...");

    // Read the complete schema fix script
    var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "complete_schema_fix.sql");
    if (!File.Exists(scriptPath))
    {
        Console.WriteLine($"Script file not found: {scriptPath}");
        return;
    }

    var sql = await File.ReadAllTextAsync(scriptPath);

    // Execute the script
    await context.Database.ExecuteSqlRawAsync(sql);

    Console.WriteLine("Schema migration completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during migration: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}