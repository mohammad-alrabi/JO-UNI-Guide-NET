using JO_UNI_Guide.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Seed SuperAdmin & Roles
// Seed SuperAdmin & Roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        // 1. إنشاء الأدوار إذا لم تكن موجودة
        string[] roles = { "SuperAdmin", "Admin", "Student" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2. جلب بيانات السوبر أدمن
        var superAdminEmail = configuration["SuperAdmin:Email"];
        var superAdminPassword = configuration["SuperAdmin:Password"];

        if (!string.IsNullOrEmpty(superAdminEmail))
        {
            var user = await userManager.FindByEmailAsync(superAdminEmail);
            if (user == null)
            {
                var newUser = new IdentityUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };

                // 3. ننتظر نجاح العملية (Succeeded) قبل إعطاء الصلاحية
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
        // هاد السطر بيمنع التطبيق إنه "يفقع" لو صار مشكلة في الداتا بيز
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();