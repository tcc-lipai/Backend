using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class ConquistaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
