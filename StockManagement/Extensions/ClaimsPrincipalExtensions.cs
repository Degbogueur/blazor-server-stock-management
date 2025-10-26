using System.Security.Claims;

namespace StockManagement.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    public static string GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }

    public static string GetUserFullName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("FullName")?.Value 
            ?? principal.Identity?.Name
            ?? "User";
    }
}
