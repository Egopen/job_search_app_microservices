using Microsoft.AspNetCore.Mvc;
using MVCFrontForJobSeek.Models;
using System.Diagnostics;

namespace MVCFrontForJobSeek.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            // Получаем уникальный идентификатор запроса
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Создаем модель для представления ошибки
            var model = new ErrorViewModel { RequestId = requestId };

            // Возвращаем представление ошибки с моделью
            return View(model);
        }
    }
}
