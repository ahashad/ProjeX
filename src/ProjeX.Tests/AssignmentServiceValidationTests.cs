using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using Xunit;
using ProjeX.Application.ActualAssignment;
using ProjeX.Application.ActualAssignment.Commands;
using ProjeX.Infrastructure.Data;
using ProjeX.Domain.Entities;
using ProjeX.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace ProjeX.Tests
{
    public class AssignmentServiceValidationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AssignmentService _assignmentService;
        private readonly Mock<IMapper> _mapperMock;
        private readonly string _testUserId = "test-user-123";

        public AssignmentServiceValidationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _assignmentService = new AssignmentService(_context, _mapperMock.Object);

            // Seed test data
            SeedTestData();
        }

        #region Date Range Validation Tests

        [Fact]
        public async Task CreateAsync_ShouldBlockAssignment_WhenStartDateBeforeProjectStart()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate.AddDays(-1), // Before project start
                AllocationPercent = 50
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Assignment start date cannot be before project start date", result.Errors);
        }

        [Fact]
        public async Task CreateAsync_ShouldBlockAssignment_WhenEndDateAfterProjectEnd()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                EndDate = TestData.ProjectEndDate.AddDays(1), // After project end
                AllocationPercent = 50
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Assignment end date cannot be after project end date", result.Errors);
        }

        [Fact]
        public async Task CreateAsync_ShouldBlockAssignment_WhenStartDateAfterEndDate()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate.AddDays(10),
                EndDate = TestData.ProjectStartDate.AddDays(5), // End before start
                AllocationPercent = 50
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Assignment start date must be before or equal to end date", result.Errors);
        }

        #endregion

        #region Allocation Validation Tests

        [Fact]
        public async Task CreateAsync_ShouldBlockAssignment_WhenAllocationExceedsPlannedSlot()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 90 // Exceeds planned 80%
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Assignment allocation (90%) exceeds planned slot allocation (80%)", result.Errors);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task CreateAsync_ShouldBlockAssignment_WhenAllocationIsZeroOrNegative(decimal allocation)
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = allocation
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Assignment allocation must be greater than 0%", result.Errors);
        }

        [Fact]
        public async Task CreateAsync_ShouldBlockAssignment_WhenEmployeeAllocationExceeds100Percent()
        {
            // Arrange - Add existing assignment
            await AddExistingAssignment(TestData.EmployeeId, 60);

            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50 // Total would be 110%
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Employee total allocation (110%) would exceed 100%", result.Errors);
        }

        #endregion

        #region Role Validation Tests

        [Fact]
        public async Task CreateAsync_ShouldBlockAssignment_WhenEmployeeRoleMismatchesPlannedSlot()
        {
            // Arrange - Create employee with different role
            var differentRoleId = Guid.NewGuid();
            var differentRole = new RolesCatalog
            {
                Id = differentRoleId,
                RoleName = "Different Role",
                Notes = "Test role",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            var employeeWithDifferentRole = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = "Different",
                LastName = "Role Employee",
                Email = "different@test.com",
                RoleId = differentRoleId,
                Role = differentRole,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.RolesCatalogs.Add(differentRole);
            _context.Employees.Add(employeeWithDifferentRole);
            await _context.SaveChangesAsync();

            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = employeeWithDifferentRole.Id,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Employee role (Different Role) does not match planned slot role", result.Errors);
        }

        #endregion

        #region Warning Validation Tests

        [Fact]
        public async Task CreateAsync_ShouldShowWarning_WhenCostVarianceExceedsThreshold()
        {
            // Arrange - Employee with higher salary than planned cost
            var employee = await _context.Employees.FindAsync(TestData.EmployeeId);
            employee!.Salary = 120000; // Higher than planned cost would suggest
            await _context.SaveChangesAsync();

            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.True(result.Warnings.Any(w => w.Contains("Cost variance", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task CreateAsync_ShouldShowWarning_WhenEmployeeUtilizationExceeds80Percent()
        {
            // Arrange - Add existing assignment to bring total close to 80%
            await AddExistingAssignment(TestData.EmployeeId, 50);

            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 40 // Total would be 90%
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.True(result.Warnings.Any(w => w.Contains("high utilization")));
        }

        #endregion

        #region Approval Workflow Tests

        [Fact]
        public async Task CreateAsync_ShouldRequireApproval_WhenCostVarianceExceedsThreshold()
        {
            // Arrange - High cost variance scenario
            var employee = await _context.Employees.FindAsync(TestData.EmployeeId);
            employee!.Salary = 150000; // Significantly higher
            await _context.SaveChangesAsync();

            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.True(result.IsSuccessful);

            // Verify assignment status
            var assignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.ProjectId == command.ProjectId &&
                                         a.EmployeeId == command.EmployeeId);

            Assert.NotNull(assignment);
            Assert.Equal(AssignmentStatus.Planned, assignment.Status); // Requires approval
            Assert.NotNull(assignment.RequestedByUserId);
        }

        [Fact]
        public async Task ApproveAsync_ShouldActivateAssignment_WhenValidApprover()
        {
            // Arrange - Create pending assignment
            var assignment = new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50,
                Status = AssignmentStatus.Planned,
                RequestedByUserId = _testUserId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.ActualAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            var approverId = "approver-123";

            // Act
            await _assignmentService.ApproveAsync(assignment.Id, approverId);

            // Assert
            var updatedAssignment = await _context.ActualAssignments.FindAsync(assignment.Id);
            Assert.Equal(AssignmentStatus.Active, updatedAssignment!.Status);
            Assert.Equal(approverId, updatedAssignment.ApprovedByUserId);
            Assert.NotNull(updatedAssignment.ApprovedOn);
        }

        [Fact]
        public async Task RejectAsync_ShouldCancelAssignment_WithReason()
        {
            // Arrange - Create pending assignment
            var assignment = new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50,
                Status = AssignmentStatus.Planned,
                RequestedByUserId = _testUserId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.ActualAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            var approverId = "approver-123";
            var rejectionReason = "Budget constraints";

            // Act
            await _assignmentService.RejectAsync(assignment.Id, approverId, rejectionReason);

            // Assert
            var updatedAssignment = await _context.ActualAssignments.FindAsync(assignment.Id);
            Assert.Equal(AssignmentStatus.Cancelled, updatedAssignment!.Status);
            Assert.Contains(rejectionReason, updatedAssignment.Notes);
        }

        #endregion

        #region Entity State Tests

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenProjectNotFound()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = Guid.NewGuid(), // Non-existent project
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _assignmentService.CreateAsync(command, _testUserId));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenEmployeeNotFound()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = Guid.NewGuid(), // Non-existent employee
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _assignmentService.CreateAsync(command, _testUserId));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenPlannedTeamSlotNotFound()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = Guid.NewGuid(), // Non-existent slot
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _assignmentService.CreateAsync(command, _testUserId));
        }

        #endregion

        #region Success Scenarios

        [Fact]
        public async Task CreateAsync_ShouldSucceed_WhenAllValidationsPass()
        {
            // Arrange
            var command = new CreateActualAssignmentCommand
            {
                ProjectId = TestData.ProjectId,
                PlannedTeamSlotId = TestData.PlannedTeamSlotId,
                EmployeeId = TestData.EmployeeId,
                StartDate = TestData.ProjectStartDate,
                AllocationPercent = 50,
                Notes = "Test assignment"
            };

            // Act
            var result = await _assignmentService.CreateAsync(command, _testUserId);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Errors);

            // Verify assignment was created
            var assignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.ProjectId == command.ProjectId &&
                                         a.EmployeeId == command.EmployeeId);

            Assert.NotNull(assignment);
            Assert.Equal(command.AllocationPercent, assignment.AllocationPercent);
            Assert.Equal(command.Notes, assignment.Notes);
            Assert.Equal(_testUserId, assignment.CreatedBy);
        }

        #endregion

        #region Helper Methods

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
                Salary = 80000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            // Create planned team slot
            var plannedSlot = new PlannedTeamSlot
            {
                Id = TestData.PlannedTeamSlotId,
                ProjectId = TestData.ProjectId,
                RoleId = TestData.RoleId,
                AllocationPercent = 80,
                PeriodMonths = 6,
                PlannedMonthlyCost = 40000,
                Status = PlannedTeamStatus.Planned,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.RolesCatalogs.Add(role);
            _context.Clients.Add(client);
            _context.Projects.Add(project);
            _context.Employees.Add(employee);
            _context.PlannedTeamSlots.Add(plannedSlot);
            _context.SaveChanges();
        }

        private async Task AddExistingAssignment(Guid employeeId, decimal allocationPercent)
        {
            var existingAssignment = new ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(), // Different project
                PlannedTeamSlotId = Guid.NewGuid(),
                EmployeeId = employeeId,
                StartDate = TestData.ProjectStartDate,
                EndDate = TestData.ProjectEndDate,
                AllocationPercent = allocationPercent,
                Status = AssignmentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _testUserId
            };

            _context.ActualAssignments.Add(existingAssignment);
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
            public static readonly Guid RoleId = Guid.NewGuid();
            public static readonly Guid PlannedTeamSlotId = Guid.NewGuid();
            public static readonly DateTime ProjectStartDate = DateTime.Today;
            public static readonly DateTime ProjectEndDate = DateTime.Today.AddMonths(6);
        }
    }
}