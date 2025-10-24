namespace StockManagement.ViewModels.Results;

public class DataGridResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
