using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StockManagement.Models;

namespace StockManagement.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task AddDefaultRolesAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "Manager" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole { Name = role });
        }
    }

    public static async Task AddAdminUserAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<AdminUserCredentials>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationUser>>();

        var email = options.Value.Email;
        var password = options.Value.Password;

        var superAdmin = await userManager.FindByEmailAsync(email);
        if (superAdmin == null)
        {
            superAdmin = new ApplicationUser
            {
                FirstName = "Sytem",
                LastName = "Admin",
                UserName = email,
                Email = email,
                IsActive = true,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdmin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "Admin");
            }
            else
            {
                logger.LogError($"Failed to create admin user", result.Errors.ToArray());
            }
        }
    }
}

internal class AdminUserCredentials
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}