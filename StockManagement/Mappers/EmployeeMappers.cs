using StockManagement.Models;
using StockManagement.ViewModels.Employees;

namespace StockManagement.Mappers;

public static class EmployeeMappers
{
    public static Employee ToModel(this CreateOrUpdateEmployeeViewModel viewModel)
    {
        return new Employee
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            Position = viewModel.Position
        };
    }
}
