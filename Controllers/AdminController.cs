using MarkRestaurant.Data.Repositories;
using MarkRestaurant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MarkRestaurant.Controllers
{
    public class AdminController : Controller
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AdminController> _logger;
        private readonly IMemoryCache _cache;

        public AdminController(
            IMenuRepository menuRepository,
            IOrderRepository orderRepository,
            IDashboardRepository dashboardRepository,
            IUserRepository userRepository,
            UserManager<User> userManager,
            ILogger<AdminController> logger,
            IMemoryCache cache
        )
        {
            _menuRepository = menuRepository;
            _orderRepository = orderRepository;
            _dashboardRepository = dashboardRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _logger = logger;
            _cache = cache;
        }

        private async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _menuRepository.GetProductById(id);
        }
        private async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        #region Add Methods

        [HttpPost]
        public async Task<IActionResult> AddProductToMenu(string category, string title, decimal price, IFormFile imageFile)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(category))
            {
                return View("Error", new ErrorViewModel("Error", "The product was not created. Invalid input data.", "Admin", "AdminAddProduct"));
            }

            var existingProduct = await _menuRepository.GetProductByTitleAndCategoryAsync(title, category);
            if (existingProduct != null)
            {
                return View("Error", new ErrorViewModel("Error", "The product already exists in the menu.", "Admin", "AdminAddProduct"));
            }

            await _menuRepository.AddProduct(category, title, price, imageFile);

            return View("~/Views/Admin/AdminMenu.cshtml");
        }

        #endregion

        #region Delete Methods

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await GetUserByEmailAsync(email);

            if (user == null)
            {
                return View("Error", new ErrorViewModel("Error", "The user was not found.", "Admin", "AdminUsers"));
            }

            var result = await _userRepository.DeleteUserFullyAsync(user.UserName!);

            if (result)
            {
                return View("~/Views/Admin/AdminUsers.cshtml");
            }

            return View("Error", new ErrorViewModel("Error", "Error occurred while deleting the user.", "Admin", "AdminUsers"));
        }
        [HttpPost]
        public async Task<IActionResult> RemoveProductFromMenu(Guid productId)
        {
            var product = await _menuRepository.GetProductById(productId);

            if (product == null)
            {
                return View("Error", new ErrorViewModel("Error", "The product was not found.", "Navigation", "AdminMenu"));
            }

            await _menuRepository.DeleteProduct(product);

            return View("~/Views/Admin/AdminMenu.cshtml");
        }
        
        #endregion
        
        #region Edit Methods

        [HttpPost]
        public async Task<IActionResult> EditUser(string email, string name, string surname, string middleName, int age, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return View("Error", new ErrorViewModel("Error", "Error occurred while editing the user.", "Admin", "AdminUsers"));
            }

            var user = await GetUserByEmailAsync(email);

            user!.Name = name;
            user.Surname = surname;
            user.MiddleName = middleName;
            user.Age = age;
            user.PhoneNumber = phoneNumber;

            _cache.Remove("all_users");

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                return View("~/Views/Admin/AdminUsers.cshtml");
            }

            return View("Error", new ErrorViewModel("Error", "Error occurred while editing the user.", "Admin", "AdminUsers"));
        }
        [HttpPost]
        public async Task<IActionResult> EditProductFromMenu(Guid id, string category, string title, decimal price, IFormFile imageFile, bool inStock) 
        {
            var product = await _menuRepository.GetProductById(id);

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(category))
            {
                return View("Error", new ErrorViewModel("Error", "The product was not edit. Invalid input data.", "Admin", "AdminEditProduct"));
            }

            await _menuRepository.EditProduct(product!, category, title, price, imageFile, inStock);

            return View("~/Views/Admin/AdminMenu.cshtml");
        }
    
        #endregion


        #region Admin Views

        public IActionResult AdminAddProduct() => View("~/Views/Admin/AdminAddProduct.cshtml");
        public IActionResult AdminDashboard() => View("~/Views/Admin/AdminDashboard.cshtml");
        public IActionResult AdminTodayCompletedOrders() => View("~/Views/Admin/AdminTodayCompletedOrders.cshtml");
        public IActionResult AdminCompletedOrders() => View("~/Views/Admin/AdminCompletedOrders.cshtml");
        public IActionResult AdminActiveOrders() => View("~/Views/Admin/AdminActiveOrders.cshtml");
        public IActionResult AdminMenu() => View("~/Views/Admin/AdminMenu.cshtml");
        public IActionResult AdminUsers() => View("~/Views/Admin/AdminUsers.cshtml");
        
        [HttpPost]
        public async Task<IActionResult> AdminEditUser(string email)
        {
            var user = await GetUserByEmailAsync(email);
            
            return View("~/Views/Admin/AdminEditUser.cshtml", user);
        }
        [HttpPost]
        public async Task<IActionResult> AdminEditProduct(Guid productId)
        {
            var product = await GetProductByIdAsync(productId);
            return View("~/Views/Admin/AdminEditProduct.cshtml", product);
        }

        #endregion
    }
}
