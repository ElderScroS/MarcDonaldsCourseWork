using MarkRestaurant.Data.Repositories;
using MarkRestaurant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MarkRestaurant.Controllers
{
    public class UserController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(
            ICardRepository cardRepository,
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IMenuRepository menuRepository,
            IUserRepository userRepository,
            IAddressRepository addressRepository,
            IEmailSender emailSender,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<UserController> logger
        )
        {
            _cardRepository = cardRepository;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _menuRepository = menuRepository;
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        #region Cart Methods

        [HttpPost]
        public async Task<IActionResult> AddProductToCart([FromBody] Guid productId)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            var product = await _menuRepository.GetProductById(productId);

            if (product == null)
            {
                return View("Error", new ErrorViewModel("Error", "The product was not found."));
            }

            int quantity = await _cartRepository.AddProductToCartOrIncreaseQuantity(user!.Id, product);
            await _cartRepository.GetTotalPrice(user.Id);

            return Ok(quantity);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProductFromCart([FromBody] Guid productId)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            var product = await _menuRepository.GetProductById(productId);

            if (product == null)
                return View("Error", new ErrorViewModel("Error", "The product was not found."));

            int quantity = await _cartRepository.RemoveProductFromCartOrDecreaseQuantity(user!.Id, productId);
            await _cartRepository.GetTotalPrice(user.Id);

            return Ok(quantity);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] string leaveAtDoor)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            bool leave = leaveAtDoor == "yes";
            Order order = await _orderRepository.CreateOrderAsync(user!, leave);

            await _emailSender.SendReceiptEmailAsync(
                order.User!.Email!,
                "Your order receipt",
                order.OrderNumber,
                order.User.Name,
                order.DeliveryCost,
                order.Distance,
                order.Items,
                order.Amount,
                order.PaymentMethod,
                order.TipsAmount
            );

            return Ok(new
            {
                orderId = order.Id,
                orderNumber = order.OrderNumber,
            });
        }

        [HttpPost]
        public async Task<IActionResult> FinishOrder([FromBody] string orderId)
        {
            _logger.LogInformation("FinishOrder called with orderId: {OrderId}", orderId);

            if (Guid.TryParse(orderId, out Guid orderID))
            {
                User? user = null;

                if (User?.Identity?.IsAuthenticated == true)
                {
                    user = await _userManager.GetUserAsync(User);

                    if (user == null)
                    {
                        _logger.LogWarning("User not found or signed out during FinishOrder.");
                        await _signInManager.SignOutAsync();
                        return RedirectToAction("Index", "Home");
                    }
                }

                var order = await _orderRepository.GetOrderByIdAsync(orderID);
                if (order == null)
                {
                    _logger.LogWarning("Order not found with ID: {OrderID}", orderID);
                    return NotFound();
                }

                await _orderRepository.FinishOrderAsync(order.Id);
                _logger.LogInformation("Order {OrderNumber} marked as finished.", order.OrderNumber);

                try
                {
                    await _cartRepository.SetTips(user!.Id, 0, 0);

                    await _emailSender.SendCompletionEmailAsync(
                        order.User!.Email!,
                        order.OrderNumber,
                        order.User.Name,
                        order.CreatedAt,
                        DateTime.UtcNow
                    );
                    _logger.LogInformation("Completion email sent to {Email}.", order.User.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send completion email for order {OrderNumber}.", order.OrderNumber);
                }

                return Ok();
            }

            _logger.LogWarning("Invalid orderId format in FinishOrder: {OrderId}", orderId);
            return BadRequest("Invalid order ID.");
        }

        #endregion

        #region Profile Methods

        [HttpPost]
        public async Task<IActionResult> SendSupportMessage([FromBody] SupportMessageViewModel model)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.Subject) || string.IsNullOrWhiteSpace(model.Message))
            {
                return BadRequest("All fields are required.");
            }

            await _emailSender.SendSupportMessageAsync(model.Subject, model.Message, model.Name, user!.Email!);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Save(string name, string surname, string middleName, int age, string phoneNumber, IFormFile profileImage, string cancelBtn)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            if (cancelBtn == "cancelBtn")
            {
                return View("~/Views/User/Profile.cshtml", user);
            }
            if (string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return View("~/Views/User/EditProfile.cshtml", user);
            }

            var userNew = await _userRepository.SaveChanges(user!, name, surname, middleName, age, phoneNumber, profileImage);

            var updateResult = await _userManager.UpdateAsync(userNew);

            if (updateResult.Succeeded)
            {
                await _signInManager.SignInAsync(user!, isPersistent: true);

                // await _orderRepository.FinishAllActiveOrdersAsync();

                return View("~/Views/User/Profile.cshtml", userNew);
            }

            return View("~/Views/User/EditProfile.cshtml", user);
        }

        public async Task<IActionResult> DeleteUser()
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            await _signInManager.SignOutAsync();
            var result = await _userRepository.DeleteUserFullyAsync(user!.UserName!);

            if (result)
            {
                return RedirectToAction("Index", "Home");
            }

            return View("Error", new ErrorViewModel("Error", "Error occurred while deleting the user.", "Home", "Index"));
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] ModelForAddAddress address)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            var newAddress = new Address
            {
                Title = address.Title,
                City = address.City,
                Street = address.Street,
                HouseNumber = address.HouseNumber,
                Entrance = address.Entrance,
                FloorApartment = address.FloorApartment,
                Comment = address.Comment,
                Latitude = address.Latitude,
                Longitude = address.Longitude
            };

            await _addressRepository.AddAddressAsync(newAddress, user!);

            await _addressRepository.SelectAddressAsync(newAddress.Id, user!);

            await _cartRepository.SetAddressCart(user!.Id);

            return View("~/Views/User/Profile.cshtml", user);
        }

        [HttpPost]
        public async Task<IActionResult> EditAddress([FromBody] ModelForEditAddress address)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                User? user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            await _addressRepository.UpdateAddressAsync(address.Id, address.Entrance!, address.FloorApartment!, address.Comment!);

            return Ok(new { succes = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAddress(Guid addressId, string currentPage)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            if (await _addressRepository.DeleteAddressAsync(addressId, user!) != true) {
                await _cartRepository.SetAddressCart(user!.Id);
            }

            if (currentPage == "Profile")
                return View("~/Views/User/Profile.cshtml", user);

            return View("~/Views/User/Cart.cshtml", user);
        }

        [HttpPost]
        public async Task<IActionResult> SelectAddress(Guid addressId, string currentPage)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            await _addressRepository.SelectAddressAsync(addressId, user!);
            await _cartRepository.SetAddressCart(user!.Id);

            if (currentPage == "Profile")
                return View("~/Views/User/Profile.cshtml", user);

            return View("~/Views/User/Cart.cshtml", user);
        }

        [HttpPost]
        public async Task<IActionResult> AddCard([FromBody] ModelForAddCard cardJS)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            Card card = new Card {
                CardName = "",
                CardNumber = cardJS.CardNumber,
                ExpiryMonth = cardJS.ExpiryMonth,
                ExpiryYear = cardJS.ExpiryYear,
                CVV = cardJS.CVV,
            };

            if (!string.IsNullOrEmpty(cardJS.CardName)) {
                card.CardName = cardJS.CardName;
            }

            string cardNumber = cardJS.CardNumber?.Replace(" ", "").Trim()!;

            if (cardNumber!.Length >= 4)
            {
                string prefix = cardNumber.Substring(0, 4);

                if (prefix.StartsWith("4"))
                {
                    card.IsVisa = true;
                }
                else if (int.TryParse(prefix, out int prefixInt))
                {
                    if ((prefixInt >= 5100 && prefixInt <= 5599) ||
                        (prefixInt >= 2221 && prefixInt <= 2720))
                    {
                        card.IsVisa = false;
                    }
                }
            }

            if (await _cardRepository.AddCardAsync(card, user!))
            {
                if (cardJS.IsSelected)
                {
                    if (await _cardRepository.SelectCardAsync(card.Id, user!))
                    {
                        return Ok(new { cardId = card.Id });
                    }
                }
                else
                {
                    await _cartRepository.SetTips(user!.Id, 0, 0);
                }

                return Ok(new { cardId = card.Id });
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SelectCard([FromBody] string cardId)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            if (Guid.TryParse(cardId, out Guid parsedGuid))
            {
                if (await _cardRepository.SelectCardAsync(parsedGuid, user!))
                    return Ok(new { succes = true });
            }

            return Ok(new { succes = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCard([FromBody] string cardId)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            if (Guid.TryParse(cardId, out Guid parsedGuid))
            {
                if (await _cardRepository.DeleteCardAsync(parsedGuid, user!))
                {
                    return Ok(new { succes = true });
                }
            }

            await _cartRepository.SetTips(user!.Id, 0, 0);

            return Ok(new { succes = false });
        }

        [HttpPost]
        public async Task<IActionResult> SelectPayment([FromBody] string payment)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            if (payment == "Apple") {
                user!.PaymentMethod = PaymentMethod.ApplePay;
            }
            else if (payment == "Google") {
                user!.PaymentMethod = PaymentMethod.GooglePay;
            }
            else {
                await _cartRepository.SetTips(user!.Id, 0, 0);
                user!.PaymentMethod = PaymentMethod.Cash;
            }

            await _cardRepository.UnSelectAllCardsAsync(user);
            await _userManager.UpdateAsync(user!);

            return Ok(new { succes = true });
        }

        [HttpPost]
        public async Task<IActionResult> SelectTips([FromBody] ModelForTips _tips)
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            await _cartRepository.SetTips(user!.Id, _tips.TipsPercentage, _tips.TipsAmount!);

            return Ok();
        }

        #endregion

        #region Logged Views

        public async Task<IActionResult> EditProfile()
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(user);
        }
        public async Task<IActionResult> Profile()
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            await _cartRepository.SetTips(user!.Id, 0, 0);

            return View(user);
        }
        public async Task<IActionResult> Cart()
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(user);
        }
        public async Task<IActionResult> OrderHistory()
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(user);
        }
        public async Task<IActionResult> Support()
        {
            User? user = null;

            if (User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);

                if (user == null) {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(user);
        }

        #endregion

        #region Models For Js

        public class SupportMessageViewModel {
            public string Name { get; set; } = null!;
            public string Subject { get; set; } = null!;
            public string Message { get; set; } = null!;
        }
        public class ModelForTips {
            public int TipsPercentage { get; set; }
            public decimal TipsAmount { get; set; }
        }
        public class ModelForAddCard
        {
            public string? CardName { get; set; }
            public string? CardNumber { get; set; }
            public string? ExpiryMonth { get; set; }
            public string? ExpiryYear { get; set; }
            public string? CVV { get; set; }
            public bool IsSelected { get; set; }
        }
        public class ModelForAddAddress
        {
            public string? Title { get; set; }
            public string? City { get; set; }
            public string? Street { get; set; }
            public string? HouseNumber { get; set; }
            public string? Entrance { get; set; }
            public string? FloorApartment { get; set; }
            public string? Comment { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
        public class ModelForEditAddress
        {
            public Guid Id { get; set; }
            public string? Entrance { get; set; }
            public string? FloorApartment { get; set; }
            public string? Comment { get; set; }
        }

        #endregion
    }
}
