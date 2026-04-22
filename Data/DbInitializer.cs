using JO_UNI_Guide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JO_UNI_Guide.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            // --- 1. Seed Roles ---
            string[] roleNames = { "SuperAdmin", "Admin", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // --- 2. Seed SuperAdmin ---
            string superAdminEmail = config["SuperAdmin:Email"] ?? "SuperAdmin12@jouni.com";
            string superAdminPassword = config["SuperAdmin:Password"] ?? "Password@1234";

            var existingUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (existingUser == null)
            {
                var superAdminUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    Name = "System Administrator",
                    GPA = 100,
                    TawjihiTrack = Models.TawjihiTrack.Business, 
                    IsOnboarded = true
                };

                var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
                }
            }

        }
    }
}