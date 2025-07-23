using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace MarkRestaurant.Controllers
{
    public class CultureController : Controller
    {
        private readonly ILogger<CultureController> _logger;

        public CultureController(ILogger<CultureController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult SetCulture(string culture, string returnUrl = "/")
        {
            // Устанавливаем куку локализации
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/"
                });

            // Дополнительная логика для неавторизованных пользователей
            if (User.Identity!.IsAuthenticated)
            {
                // Можно, например, логировать, редиректить куда-то ещё, или устанавливать флаг в TempData
                // Пример: показать уведомление на следующей странице
                TempData["CultureChanged"] = $"Язык был установлен на {culture} (незарегистрированный пользователь)";
            }

            return LocalRedirect(returnUrl);
        }
    }
}
