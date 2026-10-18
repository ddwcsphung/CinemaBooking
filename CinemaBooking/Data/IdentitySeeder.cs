using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<IdentityUser>>();

        string[] roles = { "Admin", "Customer" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                EnsureSucceeded(result, $"Tạo vai trò {roleName}");
            }
        }

        var configuration = services.GetRequiredService<IConfiguration>();
        var adminEmail = configuration["SeedAdmin:Email"]?.Trim();

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            return;
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            return;
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                adminUser, "Admin");

            EnsureSucceeded(result, "Gán vai trò Admin");
        }
    }

    private static void EnsureSucceeded(
        IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"{action} thất bại: {errors}");
        }
    }
}
