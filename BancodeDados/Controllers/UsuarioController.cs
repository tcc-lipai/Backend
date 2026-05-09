using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
