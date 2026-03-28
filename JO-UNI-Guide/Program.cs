using JO_UNI_Guide.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// استدعاء نص الاتصال من ملف appsettings.json وربطه بـ PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// تفعيل نظام Identity مع دعم الـ Roles (الصلاحيات)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // إعدادات اختيارية: مثلاً هون خلينا الباسورد ما يكون معقد كتير للتسهيل وقت التطوير
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // استدعاء الميثود اللي كتبناها
        await JO_UNI_Guide.Data.DbInitializer.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        // لطباعة أي خطأ ممكن يصير في شاشة الكونسول
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
