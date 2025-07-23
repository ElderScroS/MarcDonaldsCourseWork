using System.Text.Json;
using MarkRestaurant.Data;
using MarkRestaurant.Data.Repositories;
using MarkRestaurant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MarkRestaurant.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly MarkRestaurantDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IDashboardRepository _dashboardRepository;

        public AuthController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            MarkRestaurantDbContext context,
            IEmailSender emailSender,
            IDashboardRepository dashboardRepository
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
            _dashboardRepository = dashboardRepository;
        }

        #region Other methods

        private async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
        [HttpPost]
        public async Task<IActionResult> CheckEmailExists([FromBody] string email)
        {
            return Ok(await GetUserByEmailAsync(email) != null);
        }
        [HttpPost]
        public async Task<IActionResult> CheckUserExists([FromBody] Dictionary<string, string> userData)
        {
            if (!userData.TryGetValue("email", out string? email) || 
                !userData.TryGetValue("password", out string? password))
            {
                return BadRequest("Invalid data format");
            }

            var admin = _context.Admins.SingleOrDefault(a => a.Username == email);
            if (admin != null)
            {
                var adminHasher = new PasswordHasher<Admin>();
                var adminResult = adminHasher.VerifyHashedPassword(admin, admin.PasswordHash!, password);
                if (adminResult == PasswordVerificationResult.Success)
                {
                    return Ok(true);
                }
            }

            var user = await GetUserByEmailAsync(email);
            if (user == null || user.PasswordHash == null)
            {
                return Ok(false);
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success;

            return Ok(result);
        }

        #endregion

        #region Auth Methods

        [HttpPost]
        public async Task<IActionResult> SignIn(string email, string passwordHash)
        {
            var admin = _context.Admins.SingleOrDefault(a => a.Username == email);

            if (admin != null)
            {
                var passwordHasher = new PasswordHasher<Admin>();
                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash!, passwordHash);

                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    return View("~/Views/Admin/AdminUsers.cshtml");
                }
            }

            var user = await GetUserByEmailAsync(email);

            var result = await _signInManager.PasswordSignInAsync(email, passwordHash, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded) {
                if (!string.IsNullOrWhiteSpace(user!.Name))
                {
                    return View("~/Views/Home/Index.cshtml", user);
                }
                else if (user.EmailConfirmed != true) {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token = token }, Request.Scheme);

                    await _signInManager.SignOutAsync();

                    await _emailSender.SendConfirmationEmailAsync(email, "Confirm Your Account",
                        $"Please confirm your account by clicking the button below:<br><a class='button' href='{confirmationLink}'>Confirm Account</a>");

                    return RedirectToAction("EmailConfirm", "Auth");
                }
                else {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    return View("~/Views/User/EditProfile.cshtml", user);
                }
            }

            return View("Error", new ErrorViewModel("Error", "Invalid login attempt"));
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(string email, string passwordHash)
        {
            if (ModelState.IsValid)
            {
                var user = new User(email, passwordHash);

                var result = await _userManager.CreateAsync(user, passwordHash);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token = token }, Request.Scheme);

                    await _emailSender.SendConfirmationEmailAsync(email, "Confirm Your Account",
                        $"Please confirm your account by clicking the button below:<br><a class='button' href='{confirmationLink}'>Confirm Account</a>");

                    _dashboardRepository.InvalidateUserRelatedCache();

                    await _signInManager.SignOutAsync();

                    return RedirectToAction("EmailConfirm", "Auth");
                }

                return View("Error", new ErrorViewModel("Registration Failed", "Registration failed. Please check your details."));
            }

            return View("Error", new ErrorViewModel("Error", "Ups...Something went wrong"));
        }
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return View("Error", new ErrorViewModel("Email Confirmation Failed", "Invalid or expired confirmation token."));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await _userManager.UpdateAsync(user);

                await _signInManager.SignInAsync(user, isPersistent: true);

                return RedirectToAction("ConfirmEmailSucces", "Auth");
            }
            else
            {
                return View("Error", new ErrorViewModel("Email Confirmation Failed", "Invalid or expired confirmation token."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return View("Error", new ErrorViewModel("Empty", "Email is required."));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !user.EmailConfirmed)
            {
                return View("Error", new ErrorViewModel("Invalid email", "Invalid email or email has not been confirmed."));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPasswordView", "Auth", new { userId = user.Id, token }, Request.Scheme);

            await _emailSender.SendForgotPasswordEmailAsync(email, resetLink!);

            return RedirectToAction("ForgotPassword", "Auth");
        }
        [HttpGet]
        public async Task<IActionResult> ResetPasswordView(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error", new ErrorViewModel("Invalid ID", $"Unable to load user with ID '{userId}'."));
            }

            ViewData["Token"] = token;

            return View("ResetPasswordView", user);
        }

        #endregion
    
        #region Auth Views 

        public IActionResult ResetPasswordView() => View("~/Views/Auth/ResetPasswordView.cshtml");
        public IActionResult EmailConfirm() => View("~/Views/Auth/EmailConfirm.cshtml");
        public IActionResult ConfirmEmailSucces() => View("~/Views/Auth/ConfirmEmailSucces.cshtml");
        public IActionResult ForgotPassword() => View("~/Views/Auth/ForgotPassword.cshtml");
        public IActionResult ResetPasswordSucces() => View("~/Views/Auth/ResetPasswordSucces.cshtml");
        
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userId, string token, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error", new ErrorViewModel("Invalid ID", $"Unable to load user with ID '{userId}'."));
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (result.Succeeded)
            {
                await _emailSender.SendPasswordChangedEmailAsync(user.Email!);

                return RedirectToAction("ResetPasswordSucces", "Auth");
            }

            foreach (var error in result.Errors)
            {
                return View("Error", new ErrorViewModel("Error", error.Description));
            }

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> ResetPasswordView(string email)
        {
            var user = await GetUserByEmailAsync(email);
            return RedirectToAction("ResetPasswordView", user);
        }

        #endregion
    }
}
