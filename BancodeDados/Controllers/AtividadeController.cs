using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class AtividadeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
