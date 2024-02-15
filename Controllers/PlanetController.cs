using System.Data;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using WebTN_MVC.Services;

namespace WebTN_MVC.Controllers
{
    [Route("he-mat-troi/{action}")]
    public class PlanetController : Controller
    {
        private readonly PlanetServices _planetServices;
        private readonly ILogger<PlanetController> _logger;

        public PlanetController(PlanetServices planetServices, ILogger<PlanetController> logger)
        {
            _planetServices = planetServices;
            _logger = logger;
        }


        // GET: Planet
        [Route("danh-sach-cac-hanh-tinh.html")]
        public ActionResult Index()
        {
            return View();
        }

        [BindProperty(SupportsGet = true, Name = "action")]
        public string Name { get; set; }
        public IActionResult Mercury()
        {
            var planet = _planetServices.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        public IActionResult Venus()
        {
            var planet = _planetServices.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Earth()
        {
            var planet = _planetServices.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Mars()
        {
            var planet = _planetServices.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        [HttpGet("/sao-moc.html")]
        public IActionResult Jupiter()
        {
            var planet = _planetServices.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Saturn()
        {
            var planet = _planetServices.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Uranus()
        {
            var planet = _planetServices.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        public IActionResult Neptune()
        {
            var planet = _planetServices.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        [Route("hanhtinh/{id:int}")]
        public IActionResult PlanetInfo(int id)
        {
            var planet = _planetServices.Where(p => p.Id == id).FirstOrDefault();
            return View("Detail", planet);
        }
    }
}
