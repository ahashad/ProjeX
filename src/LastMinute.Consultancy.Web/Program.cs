using LastMinute.Consultancy.Domain;
using LastMinute.Consultancy.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LastMinute.Consultancy.Application;
using LastMinute.Consultancy.Application.Employee;
using LastMinute.Consultancy.Application.Client;
using LastMinute.Consultancy.Application.Project;
using LastMinute.Consultancy.Application.RolesCatalog;
using LastMinute.Consultancy.Application.PlannedTeamSlot;
using LastMinute.Consultancy.Application.ActualAssignment;
using LastMinute.Consultancy.Application.TimeEntry;
using LastMinute.Consultancy.Domain.Entities;
using LastMinute.Consultancy.Application.Overhead;
using LastMinute.Consultancy.Application.Deliverable;
using LastMinute.Consultancy.Application.Invoice;
using LastMinute.Consultancy.Application.ChangeRequest;
using LastMinute.Consultancy.Application.Payment;
using LastMinute.Consultancy.Application.Reports;
using LastMinute.Consultancy.Infrastructure.Services;
using LastMinute.Consultancy.Infrastructure.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Add services needed for interceptors
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();

// Add Entity Framework with audit interceptor
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
});

// Add Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();


// Add services to the container.
builder.Services.AddControllersWithViews();

// Add AutoMapper
builder.Services.AddAutoMapper(
    typeof(RolesCatalogProfile).Assembly,
    typeof(EmployeeProfile).Assembly,
    typeof(ClientProfile).Assembly,
    typeof(ProjectProfile).Assembly,
    typeof(PlannedTeamSlotProfile).Assembly,
    typeof(ActualAssignmentProfile).Assembly,
    typeof(TimeEntryProfile).Assembly,
    typeof(OverheadProfile).Assembly,
    typeof(DeliverableProfile).Assembly,
    typeof(PaymentProfile).Assembly,
    typeof(InvoiceProfile).Assembly,
    typeof(ChangeRequestProfile).Assembly);

// Register Application Services
builder.Services.AddScoped<IPlannedTeamSlotService, PlannedTeamSlotService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IDeliverableService, DeliverableService>();
builder.Services.AddScoped<IOverheadService, OverheadService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IRolesCatalogService, RolesCatalogService>();
builder.Services.AddScoped<IChangeRequestService, ChangeRequestService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
});

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    try
 {
        await DataSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    // Only use HTTPS redirection in development if explicitly configured
    var httpsPort = builder.Configuration["HTTPS_PORT"];
    if (!string.IsNullOrEmpty(httpsPort))
    {
        app.UseHttpsRedirection();
    }
}

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();;

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();















