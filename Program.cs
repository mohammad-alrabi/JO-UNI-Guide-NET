using JO_UNI_Guide.Data;
using JO_UNI_Guide.Models; // 1. أضفنا هاد الـ namespace لاستخدام ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// --- السطر السحري لحل مشكلة التوقيت في PostgreSQL ---
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession();
// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. تعديل IdentityUser إلى ApplicationUser في إعدادات الخدمات
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Seed SuperAdmin & Roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 3. تعديل UserManager للتعامل مع الموديل المطور
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        string[] roles = { "SuperAdmin", "Admin", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var superAdminEmail = configuration["SuperAdmin:Email"];
        var superAdminPassword = configuration["SuperAdmin:Password"];

        if (!string.IsNullOrEmpty(superAdminEmail))
        {
            var user = await userManager.FindByEmailAsync(superAdminEmail);
            if (user == null)
            {
                // 4. إنشاء السوبر أدمن باستخدام الكلاس الجديد
                var newUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    Name = "System Super Admin", // حقل جديد من ApplicationUser
                    EmailConfirmed = true,
                    IsOnboarded = true // السوبر أدمن معفي من الـ Onboarding
                };

                var result = await userManager.CreateAsync(newUser, superAdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "SuperAdmin");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Seed Error: " + ex.Message);
    }
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();