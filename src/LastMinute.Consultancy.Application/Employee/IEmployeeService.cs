using LastMinute.Consultancy.Application.Employee.Commands;

namespace LastMinute.Consultancy.Application.Employee
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto?> GetByIdAsync(Guid id);
        Task<EmployeeDto> CreateAsync(CreateEmployeeCommand command);
        Task UpdateAsync(UpdateEmployeeCommand command);
        Task DeleteAsync(Guid id);
    }
}
