using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjeX.Domain.Entities;
using ProjeX.Infrastructure.Data;
using ProjeX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ProjeX.Infrastructure.Services
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Seed roles
                await SeedRolesAsync(roleManager, logger);

                // Seed admin user
                await SeedAdminUserAsync(userManager, logger);

                // Seed roles catalog (basic roles only)
                await SeedRolesCatalogAsync(context, logger);

                // Seed basic clients
                await SeedBasicClientsAsync(context, logger);

                await context.SaveChangesAsync();
                logger.LogInformation("Basic database seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            var roles = new[] { "Admin", "Manager", "Employee" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation($"Created role: {role}");
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            var adminEmail = "admin@lastminute.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation($"Created admin user: {adminEmail}");
                }
                else
                {
                    logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Seed one manager user
            var managerEmail = "manager@lastminute.com";
            var managerUser = await userManager.FindByEmailAsync(managerEmail);
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(managerUser, "Manager123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                    logger.LogInformation($"Created manager user: {managerEmail}");
                }
            }
        }

        private static async Task SeedRolesCatalogAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!context.RolesCatalogs.Any())
            {
                var rolesCatalog = new[]
                {
                    new RolesCatalog 
                    { 
                        Id = Guid.NewGuid(), 
                        RoleName = "Senior Consultant", 
                        Notes = "Senior level consultant role",
                        DefaultSalary = 8000m,
                        DefaultMonthlyIncentive = 1500m,
                        CommissionPercent = 5.0m,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedBy = "System"
                    },
                    new RolesCatalog 
                    { 
                        Id = Guid.NewGuid(), 
                        RoleName = "Consultant", 
                        Notes = "Mid-level consultant role",
                        DefaultSalary = 6000m,
                        DefaultMonthlyIncentive = 1000m,
                        CommissionPercent = 3.0m,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedBy = "System"
                    },
                    new RolesCatalog 
                    { 
                        Id = Guid.NewGuid(), 
                        RoleName = "Junior Consultant", 
                        Notes = "Entry level consultant role",
                        DefaultSalary = 4000m,
                        DefaultMonthlyIncentive = 500m,
                        CommissionPercent = 2.0m,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedBy = "System"
                    },
                    new RolesCatalog 
                    { 
                        Id = Guid.NewGuid(), 
                        RoleName = "Project Manager", 
                        Notes = "Project management role",
                        DefaultSalary = 9000m,
                        DefaultMonthlyIncentive = 2000m,
                        CommissionPercent = 6.0m,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedBy = "System"
                    },
                    new RolesCatalog 
                    { 
                        Id = Guid.NewGuid(), 
                        RoleName = "Business Analyst", 
                        Notes = "Business analysis role",
                        DefaultSalary = 7000m,
                        DefaultMonthlyIncentive = 1200m,
                        CommissionPercent = 4.0m,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedBy = "System"
                    }
                };

                context.RolesCatalogs.AddRange(rolesCatalog);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded roles catalog");
            }
        }

        private static async Task SeedBasicClientsAsync(ApplicationDbContext context, ILogger logger)
        {
            if (!context.Clients.Any())
            {
                var clients = new[]
                {
                    new Client
                    {
                        Id = Guid.NewGuid(),
                        ClientName = "Demo Client Inc",
                        ContactPerson = "John Doe",
                        Email = "john.doe@democlient.com",
                        Phone = "+1-555-0101",
                        Address = "123 Demo Street, Demo City, DC 12345",
                        Status = ClientStatus.Active,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedBy = "System"
                    },
                    new Client
                    {
                        Id = Guid.NewGuid(),
                        ClientName = "Sample Corp",
                        ContactPerson = "Jane Smith",
                        Email = "jane.smith@samplecorp.com",
                        Phone = "+1-555-0102",
                        Address = "456 Sample Avenue, Sample Town, ST 67890",
                        Status = ClientStatus.Active,
                        CreatedBy = "System",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedBy = "System"
                    }
                };

                context.Clients.AddRange(clients);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded basic clients");
            }
        }
    }
}

