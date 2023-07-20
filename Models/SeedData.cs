using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using DeliveryTerminal.Data;


namespace DeliveryTerminal.Models
{
    public static class SeedData
    {
        public static async Task SeedRolesAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole("Administrator"));
            await roleManager.CreateAsync(new IdentityRole("Employee"));
            await roleManager.CreateAsync(new IdentityRole("User"));
            await roleManager.CreateAsync(new IdentityRole("Client"));
        }

        public static async Task SeedAdministratorAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new IdentityUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    IdentityResult result = await userManager.CreateAsync(defaultUser, "aaaBBB123_");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(defaultUser, "Administrator");
                        await userManager.AddToRoleAsync(defaultUser, "Employee");
                        await userManager.AddToRoleAsync(defaultUser, "User");
                    }
                }
            }
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            
        }
    }
}
