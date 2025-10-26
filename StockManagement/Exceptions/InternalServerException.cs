namespace StockManagement.Exceptions;

public class InternalServerException : BaseException
{
    public InternalServerException() : base("An unexpected error occurred") { }
}
