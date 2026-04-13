using JO_UNI_Guide.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// --- السطر السحري لحل مشكلة التوقيت في PostgreSQL ---
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // ملاحظة: بما أنكِ في مرحلة التطوير، يفضل جعل هذه false 
    // لكي لا يطلب منكِ تفعيل الإيميل حقيقةً لتتمكني من الدخول
    options.SignIn.RequireConfirmedEmail = false;

    // إعدادات الباسوورد (اختياري لجعلها أسهل للتجربة)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Seed SuperAdmin & Roles (كودك ممتاز ومكانه صحيح)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
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
                var newUser = new IdentityUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
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

app.UseAuthentication(); // مهم جداً أن تكون قبل Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();