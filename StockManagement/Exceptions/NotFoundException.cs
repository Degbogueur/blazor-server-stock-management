namespace StockManagement.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException() : base("Record not found") { }

    public NotFoundException(string? message) : base(message) { }
}
