using MangaShop.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ================== SERVICES ==================
builder.Services.AddControllersWithViews();

// 👉 Thêm DbContext (đúng cho SQL Server)
builder.Services.AddDbContext<MangaShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 👉 Thêm Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ================== MIDDLEWARE ==================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 👉 BẬT SESSION (bắt buộc)
app.UseSession();

app.UseAuthorization();

// ================== ROUTE ==================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=NVBHome}/{action=Home}/{id?}");

app.Run();
