using Microsoft.EntityFrameworkCore;
using ProjeX.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System.Data;

Console.WriteLine("Verifying PlannedTeamSlots schema...");

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true)
        .Build();

    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=ProjeXDb;Trusted_Connection=true;MultipleActiveResultSets=true";

    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    await using var context = new ApplicationDbContext(options);

    Console.WriteLine("Checking PlannedTeamSlots table columns...");

    // Check for specific columns that should exist
    string[] expectedColumns = {
        "PlannedMonthlyCost", "PlannedVendorCost", "Notes", "Status",
        "IsVendorSlot", "RequiredSkills", "Priority", "RequiredStartDate",
        "RequiredEndDate", "PathId"
    };

    foreach (var columnName in expectedColumns)
    {
        try
        {
            var exists = await context.Database.SqlQueryRaw<int>(
                $"SELECT COUNT(*) as Value FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = '{columnName}'"
            ).FirstAsync();

            if (exists > 0)
                Console.WriteLine($"✓ Column '{columnName}' exists");
            else
                Console.WriteLine($"❌ Column '{columnName}' is missing");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error checking column '{columnName}': {ex.Message}");
        }
    }

    // Get total column count
    try
    {
        var totalColumns = await context.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) as Value FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots')"
        ).FirstAsync();

        Console.WriteLine($"\nTotal columns in PlannedTeamSlots table: {totalColumns}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error getting total column count: {ex.Message}");
    }

    Console.WriteLine("\n✓ Schema verification completed");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error during verification: {ex.Message}");
    Environment.Exit(1);
}