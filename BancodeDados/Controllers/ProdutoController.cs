using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class ProdutoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
