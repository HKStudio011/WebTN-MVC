using WebTN_MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebTN_MVC.Areas.AdminCP.Controllers
{
    [Area("AdminCP")]
    [Authorize(Roles = RoleName.Administrator)]
    public class AdminCP : Controller
    {
        [Route("/admincp/")]
        public IActionResult Index()  => View();
    }
}