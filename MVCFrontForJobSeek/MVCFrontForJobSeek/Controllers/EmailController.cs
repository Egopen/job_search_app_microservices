using Microsoft.AspNetCore.Mvc;

namespace MVCFrontForJobSeek.Controllers
{
    public class EmailController : Controller
    {
        public IActionResult Index(string Token)
        {
            return View();
        }
    }
}
