using System.Net;
using WebTN_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebTN_MVC.ExtendNethods;
using WebTN_MVC.Models;
using WebTN_MVC.Services;
using WebTN_MVC.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddOptions();

builder.Services.AddSingleton<PlanetServices>();

// DBcontext
builder.Services.AddDbContext<AppDBContext>(options =>
{
    string connectString = builder.Configuration.GetConnectionString("DBContext");
    options.UseSqlServer(connectString);
});

// Add send mail services
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddSingleton<IEmailSender, SendMailService>();

// Indentity
builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

// IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất


    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;

});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login/";
    options.LogoutPath = "/logout/";
    options.AccessDeniedPath = "/khongduoctruycap.html";
});

// other service identity

builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            var gconfig = builder.Configuration.GetSection("Authentication:Google");
            options.ClientId = gconfig["ClientId"];
            options.ClientSecret = gconfig["ClientSecret"];
            // https://localhost:5001/signin-google
            options.CallbackPath = "/dang-nhap-tu-google";
        });
// .AddFacebook(options =>
// {
//     var fconfig = builder.Configuration.GetSection("Authentication:Facebook");
//     options.AppId = fconfig["AppId"];
//     options.AppSecret = fconfig["AppSecret"];
//     options.CallbackPath = "/dang-nhap-tu-facebook";
// });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewManageMenu", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.RequireRole(RoleName.Administrator);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline .
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Custom response error 400 -500
app.AddStatusCodePages();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// endpoint

app.MapAreaControllerRoute(
    name: "product",
    pattern: "{controller}/{action=Index}/{id?}",
    areaName: "ProductManage");

app.MapAreaControllerRoute(
    name: "database",
    pattern: "{controller}/{action=Index}",
    areaName: "Database");

app.MapAreaControllerRoute(
    name: "contact",
    pattern: "{controller}/{action=Index}/{id?}",
    areaName: "Contact");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


app.Run();


//// dotnet aspnet-codegenerator controller -name Contact -namespace WebTN_MVC.Areas.Contact.Controllers -m WebTN_MVC.Models.Contact.Contact -dc WebTN_MVC.Models.AppDBContext -udl -outDir Areas/Contact/ 

