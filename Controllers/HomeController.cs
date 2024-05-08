using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTN_MVC.Models;

namespace WebTN_MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDBContext _context;

    public HomeController(ILogger<HomeController> logger, AppDBContext appDBContext)
    {
        _logger = logger;
        _context = appDBContext;
    }

    public string HiHome() => "Xin chao cac ban, toi la HiHome";
    public IActionResult Index()
    {
        var products = _context.Products
                            .Include(p => p.Author)
                            .Include(p => p.Photos)
                            .Include(p => p.ProductCategoryProducts)
                            .ThenInclude(p => p.Category)
                            .AsQueryable();

        products = products.OrderByDescending(p => p.DateUpdated).Take(4);

        var posts = _context.Posts
                            .Include(p => p.Author)
                            .Include(p => p.PostCategories)
                            .ThenInclude(p => p.Category)
                            .AsQueryable();

        posts = posts.OrderByDescending(p => p.DateUpdated).Take(3);


        ViewBag.products = products;
        ViewBag.posts = posts;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
