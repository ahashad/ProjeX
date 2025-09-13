using ProjeX.Blazor.Components;
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
using Syncfusion.Blazor;

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

// Add Identity - Modified for Blazor Server
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie authentication for Blazor Server
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MVC for controllers with antiforgery support
builder.Services.AddControllersWithViews();

// Add authentication services for Blazor
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, 
    Microsoft.AspNetCore.Components.Server.ServerAuthenticationStateProvider>();

// Add Syncfusion Blazor service
builder.Services.AddSyncfusionBlazor();

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
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map controllers
app.MapControllers();

// Remove MapRazorPages() since we're using custom Blazor Identity pages

app.Run();

