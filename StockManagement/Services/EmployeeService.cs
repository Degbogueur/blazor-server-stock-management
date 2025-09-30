using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Exceptions;
using StockManagement.Mappers;
using StockManagement.ViewModels;
using StockManagement.ViewModels.Employees;

namespace StockManagement.Services;

public interface IEmployeeService
{
    Task<bool> AddNewEmployeeAsync(CreateOrUpdateEmployeeViewModel viewModel);
    Task<bool> DeleteEmployeeAsync(int id);
    Task<IEnumerable<EmployeeViewModel>> GetEmployeesListAsync();
    Task<IEnumerable<SearchResultViewModel>> SearchEmployeesAsync(string term, CancellationToken token, int maxResults = 10);
    Task<bool> UpdateEmployeeAsync(CreateOrUpdateEmployeeViewModel viewModel);

}

internal class EmployeeService(
    StockDbContext dbContext,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    public async Task<bool> AddNewEmployeeAsync(CreateOrUpdateEmployeeViewModel viewModel)
    {
        try
        {
            var sameNameExists = await dbContext.Employees
                .AsNoTracking()
                .AnyAsync(e => EF.Functions.ILike(e.FirstName, viewModel.FirstName) &&
                               EF.Functions.ILike(e.LastName, viewModel.LastName));

            if (sameNameExists)
                throw new UnauthorizedOperationException("An employee with the same name already exists");

            var employee = viewModel.ToModel();

            await dbContext.Employees.AddAsync(employee);
            var result = await dbContext.SaveChangesAsync();

            return result > 0;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, "Error while adding new employee '{SupplierName}'.", $"{viewModel.FirstName} {viewModel.LastName}");
            throw new InternalServerException();
        }
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        try
        {
            var rowsAffected = await dbContext.Employees
                .Where(e => e.Id == id &&
                           !e.Operations.Any())
                .ExecuteUpdateAsync(e => e
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedOn, DateTime.UtcNow));

            if (rowsAffected > 0) return true;

            var employeeExists = await dbContext.Employees.IgnoreQueryFilters().AnyAsync(e => e.Id == id);
            if (!employeeExists) throw new NotFoundException("Employee not found");

            throw new UnauthorizedOperationException(
                "This employee cannot be deleted: they have associated operation entries");
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro while deleting employee with ID: {id}", id);
            throw new InternalServerException();
        }
    }

    public async Task<IEnumerable<EmployeeViewModel>> GetEmployeesListAsync()
    {
        return await dbContext.Employees
            .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
            .Select(e => new EmployeeViewModel
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                FullName = e.FullName,
                Position = e.Position
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<SearchResultViewModel>> SearchEmployeesAsync(
        string term, CancellationToken token = default, int maxResults = 10)
    {
        return await dbContext.Employees
            .Where(e => EF.Functions.ILike(e.FirstName + " " + e.LastName, $"%{term}%"))
            .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
            .Take(maxResults)
            .Select(e => new SearchResultViewModel { Id = e.Id, Text = e.FullName })
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<bool> UpdateEmployeeAsync(CreateOrUpdateEmployeeViewModel viewModel)
    {
        try
        {
            var validationResult = await dbContext.Employees
                .Where(e => e.Id == viewModel.Id || 
                           (EF.Functions.ILike(e.FirstName, viewModel.FirstName) &&
                            EF.Functions.ILike(e.LastName, viewModel.LastName)))
                .Select(p => new { p.Id, p.FirstName, p.LastName })
                .AsNoTracking()
                .ToListAsync();

            var employeeExists = validationResult.Any(e => e.Id == viewModel.Id);
            if (!employeeExists) throw new NotFoundException("Employee not found");

            var sameNameExists = validationResult.Any(s =>
                string.Equals(s.FirstName, viewModel.FirstName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(s.LastName, viewModel.LastName, StringComparison.OrdinalIgnoreCase) &&
                s.Id != viewModel.Id);
            if (sameNameExists)
                throw new UnauthorizedOperationException("An employee with the same name already exists");

            var rowsAffected = await dbContext.Employees
                .Where(e => e.Id == viewModel.Id)
                .ExecuteUpdateAsync(e => e
                    .SetProperty(e => e.FirstName, viewModel.FirstName)
                    .SetProperty(e => e.LastName, viewModel.LastName)
                    .SetProperty(e => e.Position, viewModel.Position));

            return rowsAffected > 0;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating employee with ID: {id}", viewModel.Id);
            throw new InternalServerException();
        }
    }
}
