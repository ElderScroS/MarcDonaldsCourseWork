using MarkRestaurant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MarkRestaurant.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public HomeController(UserManager<User> userManager, SignInManager<User> signInManager) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {           
            User? user = null;
            
            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user != null && user.EmailConfirmed && string.IsNullOrWhiteSpace(user.Name))
                    return View("~/Views/User/EditProfile.cshtml", user);
            }
             
            return View(user);
        }     

        public IActionResult Error(string title, string description)
        {
            var errorViewModel = new ErrorViewModel(title, description);

            return View(errorViewModel);
        }
        public IActionResult Error(string title, string description, string aspController, string aspAction)
        {
            var errorViewModel = new ErrorViewModel(title, description, aspController, aspAction);

            return View(errorViewModel);
        }
    
    }
}