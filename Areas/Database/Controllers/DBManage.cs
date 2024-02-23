using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTN_MVC.Data;
using WebTN_MVC.Models;

namespace WebTN_MVC.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/{action}")]
    public class DBManage : Controller
    {
        private readonly AppDBContext _appDBContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DBManage(AppDBContext appDBContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _appDBContext = appDBContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: DBManage
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDB()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDBAsync()
        {
            var success = await _appDBContext.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xoá Database thành công." : "Không thể xoá Database!!!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApplyMigrationAsync()
        {
            await _appDBContext.Database.MigrateAsync();
            StatusMessage = "Cập nhật Database thành công.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SendDataAsync()
        {
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach (var r in roleNames)
            {
                var roleName = r.GetRawConstantValue() as string;
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // admin, pass = admin123, email = admin@example.com
            var admin = await _userManager.FindByNameAsync("admin");
            if (admin == null)
            {
                admin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(admin, password: "admin123");
                await _userManager.AddToRoleAsync(admin, RoleName.Administrator);
            }
            StatusMessage = "Vừa send data";
            return RedirectToAction("Index");
        }
    }
}
