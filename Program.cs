using System.Net;
using Microsoft.EntityFrameworkCore;
using WebTN_MVC.ExtendNethods;
using WebTN_MVC.Models;
using WebTN_MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddSingleton<PlanetServices>();

// DBcontext
builder.Services.AddDbContext<AppDBContext>(options =>
{
    string connectString = builder.Configuration.GetConnectionString("DBContext");
    options.UseSqlServer(connectString);
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

