using Microsoft.AspNetCore.Identity;

namespace HrPuSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Create roles
            string[] roleNames = { "Admin", "Manager" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user
            var adminName = "admin";
            var adminUser = await userManager.FindByNameAsync(adminName);

            if (adminUser == null)
            {
                var adminEmail = "admin@hrpu.com";
                adminUser = new IdentityUser
                {
                    UserName = adminName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            // Create manager user
            var managerName = "manager";
            var managerUser = await userManager.FindByNameAsync(managerName);

            if (managerUser == null)
            {
                var managerEmail = "manager@hrpu.com";
                managerUser = new IdentityUser
                {
                    UserName = managerName,
                    Email = managerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(managerUser, "Manager123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }
        }
    }
}