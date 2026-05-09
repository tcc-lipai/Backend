using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class CategoriaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
