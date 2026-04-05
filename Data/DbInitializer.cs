using Microsoft.AspNetCore.Identity;

namespace JO_UNI_Guide.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndSuperAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            string[] roleNames = { "SuperAdmin", "Admin", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName)) 
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            string superAdminEmail =config[ "SupeAdmin:Email"];
            string superAdminPassword = config["SuperAdmin:Password"];
            if (await userManager.FindByEmailAsync(superAdminEmail) == null)
            {
                var superAdminUser = new IdentityUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
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
