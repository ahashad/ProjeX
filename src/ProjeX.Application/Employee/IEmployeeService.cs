using ProjeX.Application.Employee.Commands;

namespace ProjeX.Application.Employee
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto?> GetByIdAsync(Guid id);
      Task<EmployeeDto> CreateAsync(CreateEmployeeCommand command, string userId);
       Task UpdateAsync(UpdateEmployeeCommand command, string userId);
        Task DeleteAsync(Guid id);
    }
}
