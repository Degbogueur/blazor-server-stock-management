namespace StockManagement.Exceptions;

public class UnauthorizedOperationException : BaseException
{
    public UnauthorizedOperationException() : base("This operation is not authorized") { }

    public UnauthorizedOperationException(string? message) : base(message) { }
}
