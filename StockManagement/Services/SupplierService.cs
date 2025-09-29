using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Exceptions;
using StockManagement.Mappers;
using StockManagement.ViewModels.Suppliers;

namespace StockManagement.Services;

public interface ISupplierService
{
    Task<bool> AddNewSupplierAsync(CreateOrUpdateSupplierViewModel viewModel);
    Task<bool> DeleteSupplierAsync(int id);
    Task<IEnumerable<SupplierViewModel>> GetSuppliersListAsync();
    Task<IEnumerable<string>> SearchSuppliersAsync(string value, CancellationToken token, int count = 10);
    Task<bool> UpdateSupplierAsync(CreateOrUpdateSupplierViewModel viewModel);
}

internal class SupplierService(
    StockDbContext dbContext,
    ILogger<SupplierService> logger) : ISupplierService
{
    public async Task<bool> AddNewSupplierAsync(CreateOrUpdateSupplierViewModel viewModel)
    {
        try
        {
            var sameNameExists = await dbContext.Suppliers
                .AsNoTracking()
                .AnyAsync(s => EF.Functions.ILike(s.Name, viewModel.Name));

            if (sameNameExists)
                throw new UnauthorizedOperationException("A supplier with the same name already exists");

            var supplier = viewModel.ToModel();

            await dbContext.Suppliers.AddAsync(supplier);
            var result = await dbContext.SaveChangesAsync();

            return result > 0;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while adding new supplier '{SupplierName}'.", viewModel.Name);
            throw new InternalServerException();
        }
    }

    public async Task<bool> DeleteSupplierAsync(int id)
    {
        try
        {
            var rowsAffected = await dbContext.Suppliers
                .Where(s => s.Id == id &&
                           !s.Operations.Any())
                .ExecuteUpdateAsync(s => s
                .SetProperty(s => s.IsDeleted, true)
                .SetProperty(s => s.DeletedOn, DateTime.UtcNow));

            if (rowsAffected > 0) return true;

            var supplierExists = await dbContext.Suppliers.IgnoreQueryFilters().AnyAsync(s => s.Id == id);
            if (!supplierExists) throw new NotFoundException("Supplier not found");

            throw new UnauthorizedOperationException(
                "This supplier cannot be deleted: they have associated operation entries");
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro while deleting supplier with ID: {id}", id);
            throw new InternalServerException();
        }
    }

    public async Task<IEnumerable<SupplierViewModel>> GetSuppliersListAsync()
    {
        return await dbContext.Suppliers
            .OrderBy(s => s.Name)
            .Select(s => new SupplierViewModel
            {
                Id = s.Id,
                Name = s.Name,
                PhoneNumber = s.PhoneNumber,
                Email = s.Email,
                Address = s.Address
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> SearchSuppliersAsync(string value, CancellationToken token, int count = 10)
    {
        return await dbContext.Suppliers
            .Where(s => EF.Functions.ILike(s.Name, $"%{value}%"))
            .OrderBy(s => s.Name)
            .Take(count)
            .Select(s => s.Name)
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<bool> UpdateSupplierAsync(CreateOrUpdateSupplierViewModel viewModel)
    {
        try
        {
            var validationResult = await dbContext.Suppliers
                .Where(s => s.Id == viewModel.Id || EF.Functions.ILike(s.Name, viewModel.Name))
                .Select(p => new { p.Id, p.Name })
                .AsNoTracking()
                .ToListAsync();

            var supplierExists = validationResult.Any(s => s.Id == viewModel.Id);
            if (!supplierExists) throw new NotFoundException("Supplier not found");

            var sameNameExists = validationResult.Any(s =>
                string.Equals(s.Name, viewModel.Name, StringComparison.OrdinalIgnoreCase) && s.Id != viewModel.Id);
            if (sameNameExists)
                throw new UnauthorizedOperationException("A supplier with the same name already exists");

            var rowsAffected = await dbContext.Suppliers
                .Where(s => s.Id == viewModel.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(s => s.Name, viewModel.Name)
                    .SetProperty(s => s.PhoneNumber, viewModel.PhoneNumber)
                    .SetProperty(s => s.Email, viewModel.Email)
                    .SetProperty(s => s.Address, viewModel.Address));

            return rowsAffected > 0;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while updating supplier with ID: {id}", viewModel.Id);
            throw new InternalServerException();
        }
    }
}
