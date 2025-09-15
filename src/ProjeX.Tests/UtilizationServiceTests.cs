using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using Xunit;
using ProjeX.Application.ResourceUtilization;
using ProjeX.Application.ActualAssignment;
using ProjeX.Infrastructure.Data;
using ProjeX.Domain.Entities;
using ProjeX.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace ProjeX.Tests
{
    public class UtilizationServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UtilizationService _utilizationService;
        private readonly Mock<IMapper> _mapperMock;
        private readonly string _testUserId = "test-user-123";

        public UtilizationServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _utilizationService = new UtilizationService(_context, _mapperMock.Object);

            // Setup AutoMapper mock
            SetupAutoMapperMock();

            // Seed test data
            SeedTestData();
        }

        #region Employee Utilization Tests

        [Fact]
        public async Task GetEmployeeUtilizationAsync_ShouldReturnCorrectUtilization_ForSingleAssignment()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);

            // Act
            var result = await _utilizationService.GetEmployeeUtilizationAsync(TestData.EmployeeId, startDate, endDate);

            // Assert
            Assert.Equal(TestData.EmployeeId, result.EmployeeId);
            Assert.Equal(60m, result.TotalAllocationPercent); // Single assignment with 60%
            Assert.Equal(1, result.ProjectCount);
            Assert.False(result.IsOverAllocated);
            Assert.True(result.IsUnderUtilized); // 60% is under 80%
        }

        [Fact]
        public async Task GetEmployeeUtilizationAsync_ShouldReturnCorrectUtilization_ForMultipleAssignments()
        {
            // Arrange
            await AddSecondAssignment(TestData.EmployeeId, 30m);
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);

            // Act
            var result = await _utilizationService.GetEmployeeUtilizationAsync(TestData.EmployeeId, startDate, endDate);

            // Assert
            Assert.Equal(90m, result.TotalAllocationPercent); // 60% + 30%
            Assert.Equal(1, result.ProjectCount); // Both assignments are for the same project
            Assert.False(result.IsOverAllocated);
            Assert.False(result.IsUnderUtilized); // 90% is optimal
        }

        [Fact]
        public async Task GetEmployeeUtilizationAsync_ShouldDetectOverAllocation()
        {
            // Arrange
            await AddSecondAssignment(TestData.EmployeeId, 50m);
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);

            // Act
            var result = await _utilizationService.GetEmployeeUtilizationAsync(TestData.EmployeeId, startDate, endDate);

            // Assert
            Assert.Equal(110m, result.TotalAllocationPercent); // 60% + 50%
            Assert.True(result.IsOverAllocated);
            Assert.False(result.IsUnderUtilized);
        }

        [Fact]
        public async Task GetEmployeeUtilizationAsync_ShouldExcludeInactiveAssignments()
        {
            // Arrange
            await AddSecondAssignment(TestData.EmployeeId, 30m, AssignmentStatus.Cancelled);
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);

            // Act
            var result = await _utilizationService.GetEmployeeUtilizationAsync(TestData.EmployeeId, startDate, endDate);

            // Assert
            Assert.Equal(60m, result.TotalAllocationPercent); // Only active assignment
            Assert.Equal(1, result.ProjectCount);
        }

        [Fact]
        public async Task GetEmployeeUtilizationAsync_ShouldRespectDateRange()
        {
            // Arrange - Assignment that ends before the query date range
            await AddAssignmentWithSpecificDates(
                TestData.EmployeeId,
                DateTime.Today.AddDays(-30),
                DateTime.Today.AddDays(-1),
                40m);

            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);

            // Act
            var result = await _utilizationService.GetEmployeeUtilizationAsync(TestData.EmployeeId, startDate, endDate);

            // Assert
            Assert.Equal(60m, result.TotalAllocationPercent); // Only current assignment
            Assert.Equal(1, result.ProjectCount);
        }

        #endregion

        #region Team Utilization Tests

        [Fact]
        public async Task GetTeamUtilizationAsync_ShouldReturnAllActiveEmployees()
        {
            // Arrange
            await AddSecondEmployee();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);

            // Act
            var result = await _utilizationService.GetTeamUtilizationAsync(startDate, endDate);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.Any(u => u.EmployeeId == TestData.EmployeeId));
            Assert.True(result.Any(u => u.EmployeeId == TestData.SecondEmployeeId));
        }

        [Fact]
        public async Task GetTeamUtilizationAsync_ShouldOrderByUtilizationDescending()
        {
            // Arrange
            await AddSecondEmployee();
            await AddSecondAssignment(TestData.SecondEmployeeId, 95m); // High utilization
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(30);

            // Act
            var result = await _utilizationService.GetTeamUtilizationAsync(startDate, endDate);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result[0].TotalAllocationPercent >= result[1].TotalAllocationPercent);
        }

        #endregion

        #region Project Utilization Tests

        [Fact]
        public async Task GetProjectUtilizationAsync_ShouldReturnCorrectProjectMetrics()
        {
            // Act
            var result = await _utilizationService.GetProjectUtilizationAsync(TestData.ProjectId);

            // Assert
            Assert.Equal(TestData.ProjectId, result.ProjectId);
            Assert.Equal("Test Project", result.ProjectName);
            Assert.Equal(80m, result.PlannedCapacity); // From planned team slot
            Assert.Equal(60m, result.ActualCapacity); // From actual assignment
            Assert.Equal(75m, result.UtilizationPercentage); // 60/80 * 100
        }

        [Fact]
        public async Task GetProjectUtilizationAsync_ShouldThrowException_WhenProjectNotFound()
        {
            // Arrange
            var nonExistentProjectId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _utilizationService.GetProjectUtilizationAsync(nonExistentProjectId));
        }

        [Fact]
        public async Task GetProjectUtilizationAsync_ShouldCalculateRoleUtilization()
        {
            // Arrange
            await AddSecondEmployee();
            await AddSecondAssignment(TestData.SecondEmployeeId, 40m);

            // Act
            var result = await _utilizationService.GetProjectUtilizationAsync(TestData.ProjectId);

            // Assert
            Assert.Single(result.RoleUtilization);
            var roleUtil = result.RoleUtilization.First();
            Assert.Equal("Software Developer", roleUtil.RoleName);
            Assert.Equal(100m, roleUtil.TotalAllocation); // 60% + 40%
            Assert.Equal(2, roleUtil.EmployeeCount);
        }

        #endregion

        #region Capacity Forecast Tests

        [Theory]
        [InlineData(TimeBucket.Weekly)]
        [InlineData(TimeBucket.Monthly)]
        [InlineData(TimeBucket.Quarterly)]
        public async Task GetCapacityForecastAsync_ShouldGenerateCorrectPeriods(TimeBucket timeBucket)
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(30);

            // Act
            var result = await _utilizationService.GetCapacityForecastAsync(startDate, endDate, timeBucket);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, forecast =>
            {
                Assert.True(forecast.PeriodStart >= startDate);
                Assert.True(forecast.PeriodEnd <= endDate.AddDays(7)); // Allow for period boundary
            });
        }

        [Fact]
        public async Task GetCapacityForecastAsync_ShouldCalculateCorrectCapacityMetrics()
        {
            // Arrange
            await AddSecondEmployee(); // 2 employees = 200% total capacity
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(7);

            // Act
            var result = await _utilizationService.GetCapacityForecastAsync(startDate, endDate, TimeBucket.Weekly);

            // Assert
            var forecast = result.First();
            Assert.Equal(200m, forecast.TotalCapacity); // 2 employees * 100%
            Assert.Equal(60m, forecast.TotalDemand); // Only one assignment
            Assert.Equal(140m, forecast.AvailableCapacity); // 200 - 60
            Assert.Equal(30m, forecast.UtilizationPercentage); // 60/200 * 100
        }

        #endregion

        #region Resource Recommendations Tests

        [Fact]
        public async Task GetResourceRecommendationsAsync_ShouldIdentifyOverAllocatedEmployees()
        {
            // Arrange
            await AddSecondAssignment(TestData.EmployeeId, 50m); // Total 110%

            // Act
            var result = await _utilizationService.GetResourceRecommendationsAsync();

            // Assert
            var overAllocationRec = result.FirstOrDefault(r => r.Type == "OverAllocation");
            Assert.NotNull(overAllocationRec);
            Assert.Equal("High", overAllocationRec.Priority);
            Assert.Equal(TestData.EmployeeId, overAllocationRec.EmployeeId);
            Assert.Contains("over-allocated", overAllocationRec.Description);
        }

        [Fact]
        public async Task GetResourceRecommendationsAsync_ShouldIdentifyUnderUtilizedEmployees()
        {
            // Arrange
            await AddSecondEmployee();
            // Second employee has no assignments, so 0% utilization

            // Act
            var result = await _utilizationService.GetResourceRecommendationsAsync();

            // Assert
            var underUtilizationRec = result.FirstOrDefault(r => r.Type == "UnderUtilization");
            Assert.NotNull(underUtilizationRec);
            Assert.Equal("Medium", underUtilizationRec.Priority);
            Assert.Equal(TestData.SecondEmployeeId, underUtilizationRec.EmployeeId);
            Assert.Contains("under-utilized", underUtilizationRec.Description);
        }

        [Fact]
        public async Task GetResourceRecommendationsAsync_ShouldOrderByPriority()
        {
            // Arrange
            await AddSecondEmployee();
            await AddSecondAssignment(TestData.EmployeeId, 50m); // Over-allocated (High priority)
            // Second employee under-utilized (Medium priority)

            // Act
            var result = await _utilizationService.GetResourceRecommendationsAsync();

            // Assert
            Assert.True(result.Count >= 2);
            var highPriorityCount = result.Count(r => r.Priority == "High");
            var mediumPriorityCount = result.Count(r => r.Priority == "Medium");

            Assert.True(highPriorityCount > 0);
            Assert.True(mediumPriorityCount > 0);

            // Verify ordering (High priority items should come first)
            var firstHighIndex = result.FindIndex(r => r.Priority == "High");
            var firstMediumIndex = result.FindIndex(r => r.Priority == "Medium");

            if (firstHighIndex >= 0 && firstMediumIndex >= 0)
            {
                Assert.True(firstHighIndex < firstMediumIndex);
            }
        }

        #endregion

        #region Helper Methods

        private void SetupAutoMapperMock()
        {
            _mapperMock.Setup(m => m.Map<List<ActualAssignmentDto>>(It.IsAny<List<ActualAssignment>>()))
                .Returns((List<ActualAssignment> assignments) =>
                    assignments.Select(a => new ActualAssignmentDto
                    {
                        Id = a.Id,
                        ProjectId = a.ProjectId,
                        EmployeeId = a.EmployeeId ?? Guid.Empty,
                        AllocationPercent = a.AllocationPercent,
                        Status = a.Status,
                        EmployeeName = "Test Employee",
                        ProjectName = "Test Project"
                    }).ToList());
        }

        private void SeedTestData()
        {
            // Create role
            var role = new RolesCatalog
            {
                Id = TestData.RoleId,
                RoleName = "Software Developer",
                Notes = "Test role",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Create client
            var client = new Client
            {
                Id = TestData.ClientId,
                ClientName = "Test Client",
                Email = "client@test.com",
                Status = Domain.Enums.ClientStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Create project
            var project = new Project
            {
                Id = TestData.ProjectId,
                ProjectName = "Test Project",
                ClientId = TestData.ClientId,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(6),
                Status = ProjectStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Create employee
            var employee = new Employee
            {
                Id = TestData.EmployeeId,
                FirstName = "Test",
                LastName = "Employee",
                Email = "employee@test.com",
                RoleId = TestData.RoleId,
                Role = role,
                Salary = 80000,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Create planned team slot
            var plannedSlot = new PlannedTeamSlot
            {
                Id = TestData.PlannedTeamSlotId,
                ProjectId = TestData.ProjectId,
                Project = project,
                RoleId = TestData.RoleId,
                Role = role,
                AllocationPercent = 80,
                PeriodMonths = 6,
                PlannedMonthlyCost = 40000,
                Status = PlannedTeamStatus.Planned,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Create actual assignment
            var assignment = new ActualAssignment
            {
                Id = TestData.AssignmentId,
                ProjectId = TestData.ProjectId,
                Project = project,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                PlannedTeamSlot = plannedSlot,
                EmployeeId = TestData.EmployeeId,
                Employee = employee,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(3),
                AllocationPercent = 60,
                Status = AssignmentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Establish navigation property relationships
            project.PlannedTeamSlots.Add(plannedSlot);
            project.ActualAssignments.Add(assignment);
            employee.ActualAssignments.Add(assignment);

            _context.RolesCatalogs.Add(role);
            _context.Clients.Add(client);
            _context.Projects.Add(project);
            _context.Employees.Add(employee);
            _context.PlannedTeamSlots.Add(plannedSlot);
            _context.ActualAssignments.Add(assignment);
            _context.SaveChanges();
        }

        private async Task AddSecondAssignment(Guid employeeId, decimal allocation, AssignmentStatus status = AssignmentStatus.Active)
        {
            var project = await _context.Projects.Include(p => p.ActualAssignments).FirstAsync(p => p.Id == TestData.ProjectId);
            var employee = await _context.Employees.Include(e => e.Role).FirstAsync(e => e.Id == employeeId);
            var plannedSlot = await _context.PlannedTeamSlots.FirstAsync(pts => pts.Id == TestData.PlannedTeamSlotId);

            var assignment = new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = TestData.ProjectId,
                Project = project,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                PlannedTeamSlot = plannedSlot,
                EmployeeId = employeeId,
                Employee = employee,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(2),
                AllocationPercent = allocation,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Establish navigation property relationships
            project.ActualAssignments.Add(assignment);
            employee.ActualAssignments.Add(assignment);

            _context.ActualAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }

        private async Task AddAssignmentWithSpecificDates(Guid employeeId, DateTime start, DateTime end, decimal allocation)
        {
            var assignment = new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = employeeId,
                StartDate = start,
                EndDate = end,
                AllocationPercent = allocation,
                Status = AssignmentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.ActualAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }

        private async Task AddSecondEmployee()
        {
            var role = await _context.RolesCatalogs.FirstAsync(r => r.Id == TestData.RoleId);

            var employee = new Employee
            {
                Id = TestData.SecondEmployeeId,
                FirstName = "Second",
                LastName = "Employee",
                Email = "second@test.com",
                RoleId = TestData.RoleId,
                Role = role,
                Salary = 85000,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        #endregion

        public void Dispose()
        {
            _context.Dispose();
        }

        private static class TestData
        {
            public static readonly Guid ProjectId = Guid.NewGuid();
            public static readonly Guid ClientId = Guid.NewGuid();
            public static readonly Guid EmployeeId = Guid.NewGuid();
            public static readonly Guid SecondEmployeeId = Guid.NewGuid();
            public static readonly Guid RoleId = Guid.NewGuid();
            public static readonly Guid PlannedTeamSlotId = Guid.NewGuid();
            public static readonly Guid AssignmentId = Guid.NewGuid();
        }
    }
}