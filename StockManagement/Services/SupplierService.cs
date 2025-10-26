using Microsoft.EntityFrameworkCore;
using StockManagement.Data;
using StockManagement.Exceptions;
using StockManagement.Mappers;
using StockManagement.ViewModels;
using StockManagement.ViewModels.Suppliers;

namespace StockManagement.Services;

public interface ISupplierService
{
    Task<bool> AddNewSupplierAsync(CreateOrUpdateSupplierViewModel viewModel);
    Task<bool> DeleteSupplierAsync(int id);
    Task<IEnumerable<SupplierViewModel>> GetSuppliersListAsync();
    Task<List<SearchResultViewModel>> SearchSuppliersAsync(string term, CancellationToken token, int maxResults = 10);
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
            var supplier = await dbContext.Suppliers
                .Include(s => s.Operations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
                throw new NotFoundException("Supplier not found");

            if (supplier.Operations.Count != 0)
                throw new UnauthorizedOperationException(
                    "This supplier cannot be deleted: they have associated operation entries");

            dbContext.Suppliers.Remove(supplier);
            var result = await dbContext.SaveChangesAsync();

            return result > 0;
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

    public async Task<List<SearchResultViewModel>> SearchSuppliersAsync(
        string term, CancellationToken token = default, int maxResults = 10)
    {
        return await dbContext.Suppliers
            .Where(s => EF.Functions.ILike(s.Name, $"%{term}%"))
            .OrderBy(s => s.Name)
            .Take(maxResults)
            .Select(s => new SearchResultViewModel { Id = s.Id, Text = s.Name })
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<bool> UpdateSupplierAsync(CreateOrUpdateSupplierViewModel viewModel)
    {
        try
        {
            var sameNameExists = await dbContext.Suppliers
                .AsNoTracking()
                .AnyAsync(s => EF.Functions.ILike(s.Name, viewModel.Name) && s.Id != viewModel.Id);

            if (sameNameExists)
                throw new UnauthorizedOperationException("A supplier with the same name already exists");

            var supplier = await dbContext.Suppliers.FindAsync(viewModel.Id);

            if (supplier == null)
                throw new NotFoundException("Supplier not found");

            supplier.Name = viewModel.Name;
            supplier.PhoneNumber = viewModel.PhoneNumber;
            supplier.Email = viewModel.Email;
            supplier.Address = viewModel.Address;

            var result = await dbContext.SaveChangesAsync();

            return result > 0;
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
