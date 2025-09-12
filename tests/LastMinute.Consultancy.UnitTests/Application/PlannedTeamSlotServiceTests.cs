using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Logging;
using LastMinute.Consultancy.Infrastructure.Data;
using LastMinute.Consultancy.Application.PlannedTeamSlot;
using LastMinute.Consultancy.Application.PlannedTeamSlot.Commands;
using LastMinute.Consultancy.Application.ActualAssignment;
using AutoMapper;
using LastMinute.Consultancy.Domain.Entities;
using LastMinute.Consultancy.Domain.Enums;
using Xunit;

namespace LastMinute.Consultancy.UnitTests.Application
{
    // Simple test logger implementation
    public class TestLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
    }

    public class PlannedTeamSlotServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PlannedTeamSlotService> _logger;
        private readonly PlannedTeamSlotService _service;

        public PlannedTeamSlotServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PlannedTeamSlotProfile>();
            });
            _mapper = config.CreateMapper();
            
            // Create a simple test logger
            _logger = new TestLogger<PlannedTeamSlotService>();
            
            _service = new PlannedTeamSlotService(_context, _mapper, _logger);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                ClientName = "Test Client",
                ContactPerson = "John Doe",
                Email = "john@test.com",
                Phone = "123-456-7890",
                Address = "123 Test St",
                Status = ClientStatus.Active,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var project = new Project
            {
                Id = Guid.NewGuid(),
                ProjectName = "Test Project",
                ClientId = client.Id,
                Client = client,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                Budget = 100000m,
                ExpectedWorkingPeriodMonths = 6,
                ProjectPrice = 150000m,
                Status = ProjectStatus.Planned,
                Notes = "Test project",
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var role = new RolesCatalog
            {
                Id = Guid.NewGuid(),
                RoleName = "Senior Developer",
                Level = 3,
                DefaultSalary = 8000m,
                DefaultMonthlyIncentive = 1000m,
                CommissionPercent = 2m,
                Notes = "Senior software developer",
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Clients.Add(client);
            _context.Projects.Add(project);
            _context.RolesCatalogs.Add(role);
            _context.SaveChanges();

            // Store IDs for tests
            TestProjectId = project.Id;
            TestRoleId = role.Id;
        }

        public Guid TestProjectId { get; private set; }
        public Guid TestRoleId { get; private set; }

        [Fact]
        public async Task CreateSlotAsync_ShouldCalculateBudgetCostCorrectly()
        {
            // Arrange
            var command = new CreatePlannedTeamSlotCommand
            {
                ProjectId = TestProjectId,
                RoleId = TestRoleId,
                PeriodMonths = 3,
                AllocationPercent = 50m,
                PlannedSalary = 8000m,
                PlannedIncentive = 1000m,
                PlannedCommissionPercent = 2m
            };

            // Act
            var result = await _service.CreateSlotAsync(command, "test-user");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.ProjectId, result.ProjectId);
            Assert.Equal(command.RoleId, result.RoleId);
            
            // Verify budget calculation: (8000 + 1000 + (2% of 150000)) * 3 months * 50% allocation
            var expectedCommission = 0.02m * 150000m; // 3000
            var expectedMonthlyCost = 8000m + 1000m + expectedCommission; // 12000
            var expectedBudgetCost = expectedMonthlyCost * 3m * 0.5m; // 18000
            
            Assert.Equal(expectedBudgetCost, result.ComputedBudgetCost);
        }

        [Fact]
        public async Task CreateSlotAsync_ShouldValidatePeriodMonths()
        {
            // Arrange
            var command = new CreatePlannedTeamSlotCommand
            {
                ProjectId = TestProjectId,
                RoleId = TestRoleId,
                PeriodMonths = 12, // Exceeds project expected period of 6 months
                AllocationPercent = 100m,
                PlannedSalary = 8000m,
                PlannedIncentive = 1000m,
                PlannedCommissionPercent = 2m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateSlotAsync(command, "test-user"));
            
            Assert.Contains("cannot exceed project expected working period", exception.Message);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ShouldReturnOnlyUnassignedSlots()
        {
            // Arrange - Create a slot first
            var command = new CreatePlannedTeamSlotCommand
            {
                ProjectId = TestProjectId,
                RoleId = TestRoleId,
                PeriodMonths = 3,
                AllocationPercent = 100m,
                PlannedSalary = 8000m,
                PlannedIncentive = 1000m,
                PlannedCommissionPercent = 2m
            };

            var slot = await _service.CreateSlotAsync(command, "test-user");

            // Act - Initially should be available
            var availableSlots = await _service.GetAvailableSlotsAsync(TestProjectId);

            // Assert
            Assert.Single(availableSlots);
            Assert.Equal(slot.Id, availableSlots.First().Id);
        }

        [Fact]
        public async Task GetRemainingAllocationSegmentsAsync_ShouldReturnCorrectRemaining()
        {
            var command = new CreatePlannedTeamSlotCommand
            {
                ProjectId = TestProjectId,
                RoleId = TestRoleId,
                PeriodMonths = 3,
                AllocationPercent = 100m,
                PlannedSalary = 8000m,
                PlannedIncentive = 1000m,
                PlannedCommissionPercent = 2m
            };

            var slot = await _service.CreateSlotAsync(command, "test-user");

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com",
                Phone = "1234567890",
                RoleId = TestRoleId,
                Salary = 5000m,
                MonthlyIncentive = 500m,
                CommissionPercent = 1m,
                HireDate = DateTime.UtcNow,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _context.Employees.Add(employee);

            _context.ActualAssignments.Add(new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = TestProjectId,
                PlannedTeamSlotId = slot.Id,
                EmployeeId = employee.Id,
                AllocationPercent = 40m,
                Status = AssignmentStatus.Active,
                CostCheckWarning = false,
                CostDifferenceAmount = 0m,
                Notes = string.Empty,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            await _context.SaveChangesAsync();

            var result = await _service.GetRemainingAllocationSegmentsAsync(TestProjectId);

            Assert.True(result.ContainsKey(slot.Id));
            Assert.Equal(60m, result[slot.Id]);
        }

        [Fact]
        public async Task GetSlotsByProjectAsync_ShouldIncludeRemainingAllocation()
        {
            var command = new CreatePlannedTeamSlotCommand
            {
                ProjectId = TestProjectId,
                RoleId = TestRoleId,
                PeriodMonths = 3,
                AllocationPercent = 100m,
                PlannedSalary = 8000m,
                PlannedIncentive = 1000m,
                PlannedCommissionPercent = 2m
            };

            var slot = await _service.CreateSlotAsync(command, "test-user");

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane@test.com",
                Phone = "1234567890",
                RoleId = TestRoleId,
                Salary = 5000m,
                MonthlyIncentive = 500m,
                CommissionPercent = 1m,
                HireDate = DateTime.UtcNow,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            _context.Employees.Add(employee);

            _context.ActualAssignments.Add(new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = TestProjectId,
                PlannedTeamSlotId = slot.Id,
                EmployeeId = employee.Id,
                AllocationPercent = 40m,
                Status = AssignmentStatus.Active,
                CostCheckWarning = false,
                CostDifferenceAmount = 0m,
                Notes = string.Empty,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow,
                IsDeleted = false
            });
            await _context.SaveChangesAsync();

            var slots = await _service.GetSlotsByProjectAsync(TestProjectId);
            var dto = slots.First(s => s.Id == slot.Id);

            Assert.Equal(60m, dto.RemainingAllocationPercent);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}