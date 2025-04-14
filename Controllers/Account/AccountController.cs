using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNet.Data;
using QuanLyNet.Models;

namespace QuanLyNet.Controllers.Account
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public const string SessionKeyUserId = "_UserId";
        public const string SessionKeyUsername = "_Username";
        public const string SessionKeyIsAdmin = "_IsAdmin";
        public const string SessionKeyUserBalance = "_UserBalance";
        private const string AdminUsername = "admin";
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString(SessionKeyUsername) != null)
            {
                if (HttpContext.Session.GetString(SessionKeyIsAdmin) == "True")
                {
                    return RedirectToAction("Index", "Admin"); 
                }
                else
                {
                    return RedirectToAction("Index", "User"); 
                }
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string Username, string Password) 
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Vui lòng nhập tên đăng nhập và mật khẩu.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == Username);

            if (user != null && user.Password == Password)
            {
                HttpContext.Session.SetInt32(SessionKeyUserId, user.Id);
                HttpContext.Session.SetString(SessionKeyUsername, user.Username);
                HttpContext.Session.SetString(SessionKeyIsAdmin, user.isAdmin.ToString());
                HttpContext.Session.SetString(SessionKeyUserBalance, user.Balance.ToString());
                if (user.isAdmin)
                {
                    return RedirectToAction("Index", "Admin"); 
                }
                else
                {
                    return RedirectToAction("Index", "User");
                }
            }
            else
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View();
            }
        }
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString(SessionKeyUsername) != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(User u, string confirmPassword)
        {
            if (HttpContext.Session.GetString(SessionKeyUsername) != null)
            {
                return RedirectToAction("Index", "Home");
            }
            bool usernameExists = await _context.Users.AnyAsync(db => db.Username == u.Username);
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã được sử dụng.");
            }
            if (u.Password != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
            }
            if (ModelState.IsValid)
            {
                u.isAdmin = false; 
                u.Balance = 0;
                _context.Users.Add(u);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(u);
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Index", "Home");
        }
    }
}
