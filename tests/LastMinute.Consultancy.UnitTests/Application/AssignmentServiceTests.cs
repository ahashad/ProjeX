using AutoMapper;
using LastMinute.Consultancy.Application.ActualAssignment;
using LastMinute.Consultancy.Application.ActualAssignment.Commands;
using LastMinute.Consultancy.Application.Client;
using LastMinute.Consultancy.Application.Employee;
using LastMinute.Consultancy.Application.PlannedTeamSlot;
using LastMinute.Consultancy.Application.Project;
using LastMinute.Consultancy.Domain.Entities;
using LastMinute.Consultancy.Domain.Enums;
using LastMinute.Consultancy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LastMinute.Consultancy.UnitTests.Application
{
    public class AssignmentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly AssignmentService _service;
        private Guid _projectId;
        private Guid _slotId;
        private Guid _employeeId;

        public AssignmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ActualAssignmentProfile>();
                cfg.AddProfile<PlannedTeamSlotProfile>();
                cfg.AddProfile<ProjectProfile>();
                cfg.AddProfile<EmployeeProfile>();
                cfg.AddProfile<ClientProfile>();
            });
            _mapper = config.CreateMapper();

            _service = new AssignmentService(_context, _mapper);

            SeedData();
        }

        private void SeedData()
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                ClientName = "Test Client",
                ContactPerson = "John Doe",
                Email = "john@test.com",
                Phone = "123456789",
                Address = "Address",
                Status = ClientStatus.Active,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow
            };
            _context.Clients.Add(client);

            var project = new Project
            {
                Id = Guid.NewGuid(),
                ProjectName = "Project",
                ClientId = client.Id,
                Client = client,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(30),
                Budget = 1000m,
                Status = ProjectStatus.InProgress,
                ExpectedWorkingPeriodMonths = 1,
                ProjectPrice = 2000m,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow
            };
            _context.Projects.Add(project);
            _projectId = project.Id;

            var role = new RolesCatalog
            {
                Id = Guid.NewGuid(),
                RoleName = "Dev",
                Level = 1,
                DefaultSalary = 100m,
                DefaultMonthlyIncentive = 10m,
                CommissionPercent = 1m,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow
            };
            _context.RolesCatalogs.Add(role);

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = "Emp",
                LastName = "One",
                Email = "emp@test.com",
                Phone = "000",
                RoleId = role.Id,
                Role = role,
                HireDate = DateTime.UtcNow.Date,
                Salary = 100m,
                MonthlyIncentive = 10m,
                CommissionPercent = 1m,
                IsActive = true,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow
            };
            _context.Employees.Add(employee);
            _employeeId = employee.Id;

            var slot = new PlannedTeamSlot
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Project = project,
                RoleId = role.Id,
                Role = role,
                PeriodMonths = 1,
                AllocationPercent = 50m,
                PlannedSalary = 100m,
                PlannedIncentive = 10m,
                PlannedCommissionPercent = 1m,
                ComputedBudgetCost = 60m,
                CreatedBy = "test",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "test",
                ModifiedAt = DateTime.UtcNow
            };
            _context.PlannedTeamSlots.Add(slot);
            _slotId = slot.Id;

            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateAsync_ShouldPreventOverlappingAssignmentsForSameSlot()
        {
            var command1 = new CreateActualAssignmentCommand
            {
                ProjectId = _projectId,
                PlannedTeamSlotId = _slotId,
                EmployeeId = _employeeId,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(10),
                AllocationPercent = 50m
            };
            var res1 = await _service.CreateAsync(command1, "tester");
            Assert.NotNull(res1.Assignment);

            var command2 = new CreateActualAssignmentCommand
            {
                ProjectId = _projectId,
                PlannedTeamSlotId = _slotId,
                EmployeeId = _employeeId,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(15),
                AllocationPercent = 50m
            };

            var res2 = await _service.CreateAsync(command2, "tester");
            Assert.Null(res2.Assignment);
            Assert.NotNull(res2.ConflictingAssignment);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
