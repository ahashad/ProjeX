using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using Xunit;
using ProjeX.Application.ActualAssignment;
using ProjeX.Application.ActualAssignment.Commands;
using ProjeX.Application.ResourceUtilization;
using ProjeX.Infrastructure.Data;
using ProjeX.Domain.Entities;
using ProjeX.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace ProjeX.Tests
{
    public class WorkflowIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AssignmentService _assignmentService;
        private readonly UtilizationService _utilizationService;
        private readonly Mock<IMapper> _mapperMock;
        private readonly string _testUserId = "test-user-123";
        private readonly string _managerId = "manager-456";

        public WorkflowIntegrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mapperMock = new Mock<IMapper>();

            _assignmentService = new AssignmentService(_context, _mapperMock.Object);
            _utilizationService = new UtilizationService(_context, _mapperMock.Object);

            // Setup AutoMapper mock
            SetupAutoMapperMock();

            // Seed test data
            SeedTestData();
        }

        #region End-to-End Workflow Tests

        [Fact]
        public async Task CompleteAssignmentWorkflow_ShouldWork_WithApprovalRequired()
        {
            // Arrange - Scenario requiring approval (high cost variance)
            var employee = await _context.Employees.FindAsync(TestData.EmployeeId);
            employee!.Salary = 600000; // High salary to trigger approval (50000 monthly vs 40000 planned)
            await _context.SaveChangesAsync();

            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 70,
                Notes = "High-priority assignment"
            };

            // Act 1: Create assignment (should require approval)
            var createResult = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert 1: Assignment created but requires approval
            Assert.True(createResult.IsSuccessful);
            Assert.True(createResult.Warnings.Any(w => w.Contains("Cost variance")));

            var assignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.ProjectId == command.ProjectId && a.EmployeeId == command.EmployeeId);

            Assert.NotNull(assignment);
            Assert.Equal(AssignmentStatus.Planned, assignment.Status);
            Assert.Equal(_testUserId, assignment.RequestedByUserId);

            // Act 2: Manager approves assignment
            await _assignmentService.ApproveAsync(assignment.Id, _managerId);

            // Assert 2: Assignment is now active
            var approvedAssignment = await _context.ActualAssignments.FindAsync(assignment.Id);
            Assert.Equal(AssignmentStatus.Active, approvedAssignment!.Status);
            Assert.Equal(_managerId, approvedAssignment.ApprovedByUserId);
            Assert.NotNull(approvedAssignment.ApprovedOn);

            // Act 3: Check utilization impact
            var utilization = await _utilizationService.GetEmployeeUtilizationAsync(
                TestData.EmployeeId, TestData.ProjectStartDate, TestData.ProjectStartDate.AddDays(30));

            // Assert 3: Utilization reflects the approved assignment
            Assert.Equal(70m, utilization.TotalAllocationPercent);
            Assert.Equal(1, utilization.ProjectCount);
            Assert.False(utilization.IsOverAllocated);
        }

        [Fact]
        public async Task CompleteAssignmentWorkflow_ShouldWork_WithRejection()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50
            };

            // Act 1: Create assignment
            var createResult = await _assignmentService.CreateAsync(command, _testUserId);
            Assert.True(createResult.IsSuccessful);

            var assignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.ProjectId == command.ProjectId && a.EmployeeId == command.EmployeeId);

            // Act 2: Manager rejects assignment
            var rejectionReason = "Resource needed elsewhere";
            await _assignmentService.RejectAsync(assignment!.Id, _managerId, rejectionReason);

            // Assert: Assignment is cancelled
            var rejectedAssignment = await _context.ActualAssignments.FindAsync(assignment.Id);
            Assert.Equal(AssignmentStatus.Cancelled, rejectedAssignment!.Status);
            Assert.Contains(rejectionReason, rejectedAssignment.Notes);

            // Act 3: Check utilization (should not include cancelled assignment)
            var utilization = await _utilizationService.GetEmployeeUtilizationAsync(
                TestData.EmployeeId, TestData.ProjectStartDate, TestData.ProjectStartDate.AddDays(30));

            // Assert: Utilization should be 0 (no active assignments)
            Assert.Equal(0m, utilization.TotalAllocationPercent);
            Assert.Equal(0, utilization.ProjectCount);
        }

        [Fact]
        public async Task MultipleAssignmentsWorkflow_ShouldHandleOverAllocation()
        {
            // Arrange - Create first assignment
            var firstCommand = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 60
            };

            // Act 1: Create first assignment
            var firstResult = await _assignmentService.CreateAsync(firstCommand, _testUserId);
            Assert.True(firstResult.IsSuccessful);

            // Approve first assignment
            var firstAssignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.ProjectId == firstCommand.ProjectId);
            await _assignmentService.ApproveAsync(firstAssignment!.Id, _managerId);

            // Arrange - Create second project and slot for second assignment
            await CreateSecondProjectWithSlot();

            var secondCommand = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.SecondProjectId,
                PlannedTeamSlotId = TestData.SecondPlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50 // Total would be 110%
            };

            // Act 2: Attempt second assignment (should be blocked)
            var secondResult = await _assignmentService.CreateAsync(secondCommand, _testUserId);

            // Assert: Second assignment should be blocked due to over-allocation
            Assert.False(secondResult.IsSuccessful);
            Assert.Contains("Employee total allocation (110%) would exceed 100%", secondResult.Errors);

            // Act 3: Check utilization recommendations
            var recommendations = await _utilizationService.GetResourceRecommendationsAsync();

            // Assert: Should identify the employee as over-allocated if we had allowed it
            var utilization = await _utilizationService.GetEmployeeUtilizationAsync(
                TestData.EmployeeId, TestData.ProjectStartDate, TestData.ProjectStartDate.AddDays(30));

            Assert.Equal(60m, utilization.TotalAllocationPercent); // Only first assignment
            Assert.Equal(1, utilization.ProjectCount);
        }

        [Fact]
        public async Task TeamCapacityWorkflow_ShouldProvideAccurateForecast()
        {
            // Arrange - Create multiple employees and assignments
            await CreateMultipleEmployeesWithAssignments();

            var startDate = TestData.ProjectStartDate;
            var endDate = startDate.AddDays(30);

            // Act 1: Get team utilization
            var teamUtilization = await _utilizationService.GetTeamUtilizationAsync(startDate, endDate);

            // Assert 1: Should have multiple employees
            Assert.True(teamUtilization.Count >= 2);

            // Act 2: Get capacity forecast
            var forecast = await _utilizationService.GetCapacityForecastAsync(startDate, endDate, TimeBucket.Weekly);

            // Assert 2: Forecast should reflect team capacity
            Assert.NotEmpty(forecast);
            var firstWeek = forecast.First();
            Assert.True(firstWeek.TotalCapacity > 0);
            Assert.True(firstWeek.TotalDemand >= 0);
            Assert.Equal(firstWeek.TotalCapacity - firstWeek.TotalDemand, firstWeek.AvailableCapacity);

            // Act 3: Get recommendations
            var recommendations = await _utilizationService.GetResourceRecommendationsAsync();

            // Assert 3: Should provide actionable recommendations
            Assert.NotNull(recommendations);
            // Specific assertions depend on the test data setup
        }

        [Fact]
        public async Task ProjectUtilizationWorkflow_ShouldTrackPlannedVsActual()
        {
            // Arrange - Create assignment with different allocation than planned
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 60 // Planned was 80%
            };

            // Act 1: Create and approve assignment
            var result = await _assignmentService.CreateAsync(command, _testUserId);
            Assert.True(result.IsSuccessful);

            var assignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.ProjectId == command.ProjectId);
            await _assignmentService.ApproveAsync(assignment!.Id, _managerId);

            // Act 2: Get project utilization
            var projectUtilization = await _utilizationService.GetProjectUtilizationAsync(TestData.ProjectId);

            // Assert: Should show variance between planned and actual
            Assert.Equal(80m, projectUtilization.PlannedCapacity); // From planned slot
            Assert.Equal(60m, projectUtilization.ActualCapacity); // From actual assignment
            Assert.Equal(75m, projectUtilization.UtilizationPercentage); // 60/80 * 100

            // Should indicate under-utilization relative to plan
            Assert.True(projectUtilization.ActualCapacity < projectUtilization.PlannedCapacity);
        }

        #endregion

        #region Validation Rule Integration Tests

        [Fact]
        public async Task ValidationRules_ShouldWorkTogether_InComplexScenarios()
        {
            // Scenario: Employee with existing assignment tries to get another that would
            // cause over-allocation AND has role mismatch AND has date conflicts

            // Arrange 1: Create existing assignment
            var existingCommand = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 70
            };

            var existingResult = await _assignmentService.CreateAsync(existingCommand, _testUserId);
            Assert.True(existingResult.IsSuccessful);

            var existingAssignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.ProjectId == existingCommand.ProjectId);
            await _assignmentService.ApproveAsync(existingAssignment!.Id, _managerId);

            // Arrange 2: Create different role and project slot
            var differentRole = new RolesCatalog
            {
                Id = Guid.NewGuid(),
                RoleName = "Project Manager",
                Notes = "Test role",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            var differentProject = new Project
            {
                Id = Guid.NewGuid(),
                ProjectName = "Different Project",
                ClientId = TestData.ClientId,
                StartDate = TestData.ProjectStartDate.AddDays(-5), // Starts earlier
                EndDate = TestData.ProjectEndDate.AddDays(10), // Ends later
                Status = ProjectStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            var differentSlot = new PlannedTeamSlot
            {
                Id = Guid.NewGuid(),
                ProjectId = differentProject.Id,
                RoleId = differentRole.Id,
                AllocationPercent = 50,
                PeriodMonths = 6,
                Status = PlannedTeamStatus.Planned,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.RolesCatalogs.Add(differentRole);
            _context.Projects.Add(differentProject);
            _context.PlannedTeamSlots.Add(differentSlot);
            await _context.SaveChangesAsync();

            // Act: Try to create conflicting assignment
            var conflictingCommand = new CreateActualAssignmentCommand
            {
                ProjectId = differentProject.Id,
                PlannedTeamSlotId = differentSlot.Id,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate.AddDays(-10), // Before project start
                AllocationPercent = 40 // Would cause over-allocation (70% + 40% = 110%)
            };

            var conflictResult = await _assignmentService.CreateAsync(conflictingCommand, _testUserId);

            // Assert: Should be blocked by multiple validation rules
            Assert.False(conflictResult.IsSuccessful);
            Assert.True(conflictResult.Errors.Count >= 2); // Multiple validation failures

            // Should include specific error messages
            Assert.Contains(conflictResult.Errors, e => e.Contains("start date"));
            Assert.Contains(conflictResult.Errors, e => e.Contains("allocation") && e.Contains("exceed"));
            Assert.Contains(conflictResult.Errors, e => e.Contains("role") && e.Contains("does not match"));
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
                StartDate = TestData.ProjectStartDate,
                EndDate = TestData.ProjectEndDate,
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

            // Establish navigation property relationships
            project.PlannedTeamSlots.Add(plannedSlot);

            _context.RolesCatalogs.Add(role);
            _context.Clients.Add(client);
            _context.Projects.Add(project);
            _context.Employees.Add(employee);
            _context.PlannedTeamSlots.Add(plannedSlot);
            _context.SaveChanges();
        }

        private async Task CreateSecondProjectWithSlot()
        {
            var secondProject = new Project
            {
                Id = TestData.SecondProjectId,
                ProjectName = "Second Project",
                ClientId = TestData.ClientId,
                StartDate = TestData.ProjectStartDate,
                EndDate = TestData.ProjectEndDate,
                Status = ProjectStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            var secondSlot = new PlannedTeamSlot
            {
                Id = TestData.SecondPlannedTeamSlotId,
                ProjectId = TestData.SecondProjectId,
                RoleId = TestData.RoleId,
                AllocationPercent = 60,
                PeriodMonths = 4,
                PlannedMonthlyCost = 30000,
                Status = PlannedTeamStatus.Planned,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.Projects.Add(secondProject);
            _context.PlannedTeamSlots.Add(secondSlot);
            await _context.SaveChangesAsync();
        }

        private async Task CreateMultipleEmployeesWithAssignments()
        {
            // Create second employee
            var secondEmployee = new Employee
            {
                Id = TestData.SecondEmployeeId,
                FirstName = "Second",
                LastName = "Employee",
                Email = "second@test.com",
                RoleId = TestData.RoleId,
                Salary = 90000,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.Employees.Add(secondEmployee);

            // Create assignment for first employee
            var firstAssignment = new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                EndDate = TestData.ProjectEndDate,
                AllocationPercent = 80,
                Status = AssignmentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Create second project for second employee
            await CreateSecondProjectWithSlot();

            var secondAssignment = new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = TestData.SecondProjectId,
                PlannedTeamSlotId = TestData.SecondPlannedTeamSlotId,
                EmployeeId = TestData.SecondEmployeeId,
                StartDate = TestData.ProjectStartDate,
                EndDate = TestData.ProjectEndDate,
                AllocationPercent = 40,
                Status = AssignmentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.ActualAssignments.Add(firstAssignment);
            _context.ActualAssignments.Add(secondAssignment);
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
            public static readonly Guid SecondProjectId = Guid.NewGuid();
            public static readonly Guid ClientId = Guid.NewGuid();
            public static readonly Guid EmployeeId = Guid.NewGuid();
            public static readonly Guid SecondEmployeeId = Guid.NewGuid();
            public static readonly Guid RoleId = Guid.NewGuid();
            public static readonly Guid PlannedTeamSlotId = Guid.NewGuid();
            public static readonly Guid SecondPlannedTeamSlotId = Guid.NewGuid();
            public static readonly DateTime ProjectStartDate = DateTime.Today;
            public static readonly DateTime ProjectEndDate = DateTime.Today.AddMonths(6);
        }
    }
}