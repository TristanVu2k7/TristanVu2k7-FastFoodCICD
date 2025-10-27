using Assignment_NET104.Data;
using Assignment_NET104.Models;
using Assignment_NET104.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =================== DB + Identity ===================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// =================== Đăng ký Services ===================
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Cấu hình Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// =================== Authentication: Google ===================
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
                           ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                               ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
        options.CallbackPath = "/signin-google";
    });


// =================== MVC + Session ===================
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session tồn tại 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cho phép upload file lớn (tối đa 50MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
    options.MemoryBufferThreshold = 52428800;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 52428800;
});
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
var app = builder.Build();

// =================== Khởi tạo thư mục upload ===================
var webHostEnvironment = app.Services.GetRequiredService<IWebHostEnvironment>();
var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "foods");
if (!Directory.Exists(uploadsFolder))
{
    Directory.CreateDirectory(uploadsFolder);
}

// =================== Middleware ===================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Bật session
app.UseSession();

// =================== Routes ===================
app.MapControllerRoute(
    name: "customers",
    pattern: "Customers/{action=Index}/{id?}",
    defaults: new { controller = "Customers" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "food",
    pattern: "Food/{action=Index}/{id?}",
    defaults: new { controller = "Food" });
app.MapControllerRoute(
    name: "adminfood",
    pattern: "AdminFood/{action=Index}/{id?}",
    defaults: new { controller = "AdminFood" });
app.MapControllerRoute(
    name: "order",
    pattern: "{controller=Order}/{action=History}/{id?}");

app.Run();
