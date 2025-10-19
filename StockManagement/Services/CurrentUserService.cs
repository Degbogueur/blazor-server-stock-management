using System.Security.Claims;

namespace StockManagement.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
}

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => httpContextAccessor.HttpContext?.User?.Identity?.Name;
}
