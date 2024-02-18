using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTN_MVC.Models;

namespace WebTN_MVC.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/{action}")]
    public class DBManage : Controller
    {
        private readonly AppDBContext _appDBContext;
        [TempData]
        public string StatusMessage { get; set; }
        public DBManage(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

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
    }
}
