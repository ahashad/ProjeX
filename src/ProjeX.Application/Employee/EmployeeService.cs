using AutoMapper;
using ProjeX.Application.Employee.Commands;
using LastMinute.Consultancy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjeX.Application.Employee
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EmployeeService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<EmployeeDto>> GetAllAsync()
        {
            var employees = await _context.Employees.Include(e => e.Role).ToListAsync();
            return _mapper.Map<List<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto?> GetByIdAsync(Guid id)
        {
            var employee = await _context.Employees.Include(e => e.Role).FirstOrDefaultAsync(e => e.Id == id);
            return employee != null ? _mapper.Map<EmployeeDto>(employee) : null;
        }

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeCommand command)
        {
            var entity = new Domain.Entities.Employee
            {
                Id = Guid.NewGuid(),
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                Phone = command.Phone,
                RoleId = command.RoleId,
                HireDate = command.HireDate,
                Salary = command.Salary,
                MonthlyIncentive = command.MonthlyIncentive,
                CommissionPercent = command.CommissionPercent,
                IsActive = command.IsActive
            };

            _context.Employees.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<EmployeeDto>(entity);
        }

        public async Task UpdateAsync(UpdateEmployeeCommand command)
        {
            var entity = await _context.Employees.FindAsync(command.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Employee with ID {command.Id} not found.");

            entity.FirstName = command.FirstName;
            entity.LastName = command.LastName;
            entity.Email = command.Email;
            entity.Phone = command.Phone;
            entity.RoleId = command.RoleId;
            entity.HireDate = command.HireDate;
            entity.Salary = command.Salary;
            entity.MonthlyIncentive = command.MonthlyIncentive;
            entity.CommissionPercent = command.CommissionPercent;
            entity.IsActive = command.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Employees.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Employee with ID {id} not found.");

            _context.Employees.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
