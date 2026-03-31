using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Service;

public static class AdminBootstrapperService
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        if (!configuration.GetValue("AdminBootstrap:Enabled", false))
            return;

        var adminEmail = configuration["AdminBootstrap:Email"];
        var adminPassword = configuration["AdminBootstrap:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Admin bootstrap is enabled but AdminBootstrap:Email or AdminBootstrap:Password is missing.");
            return;
        }

        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<Guid>
            {
                Name = "Admin",
                NormalizedName = "ADMIN"
            });
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create Admin role: {Errors}",
                                 string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            existingAdmin = new AppUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(existingAdmin, adminPassword);
            if (!createResult.Succeeded)
            {
                logger.LogError("Failed to create bootstrap admin user: {Errors}",
                                 string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Bootstrap admin user created for {AdminEmail}.", adminEmail);
        }

        if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
        {
            var addRoleResult = await userManager.AddToRoleAsync(existingAdmin, "Admin");
            if (!addRoleResult.Succeeded)
            {
                logger.LogError("Failed to assign Admin role to {AdminEmail}: {Errors}",
                                 adminEmail, string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                return;
            }
        }
    }
}