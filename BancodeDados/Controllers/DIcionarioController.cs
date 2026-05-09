using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class DIcionarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
