using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using QuanLyNet.Data;
using QuanLyNet.Models;

namespace QuanLyNet.Controllers.Account
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        private bool IsUserLoggedIn(out int? userId) 
        {
            userId = HttpContext.Session.GetInt32(AccountController.SessionKeyUserId); 
            return userId != null;
        }
        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetString(AccountController.SessionKeyUsername) != null;
        }

        public IActionResult Index()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Message = "Đây là trang dành cho người dùng thông thường.";
            ViewBag.Username = HttpContext.Session.GetString(AccountController.SessionKeyUsername);
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsUserLoggedIn(out int? userId) || userId == null) 
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem hồ sơ.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId.Value); 

            if (user == null)
            {
                HttpContext.Session.Clear();
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }
        [HttpPost] 
        public async Task<IActionResult> UpdateProfile(User updatedUser) 
        {
            if (!IsUserLoggedIn(out int? sessionUserId) || sessionUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (updatedUser.Id != sessionUserId.Value)
            {
                TempData["ErrorMessage"] = "Lỗi bảo mật: Bạn không có quyền sửa hồ sơ này.";
                return RedirectToAction("Index", "Home");
            }
            ModelState.Remove(nameof(updatedUser.Username)); 
            ModelState.Remove(nameof(updatedUser.Password));
            if (string.IsNullOrWhiteSpace(updatedUser.Name) || updatedUser.Name.Length < 2)
            {
                ModelState.AddModelError("Name", "Tên người dùng không hợp lệ (ít nhất 2 ký tự).");
            }
            if (updatedUser.PhoneNumber != null && updatedUser.PhoneNumber.Length > 15)
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại không được vượt quá 15 ký tự.");
            }

            if (ModelState.IsValid) 
            {
                var userFromDb = await _context.Users.FindAsync(updatedUser.Id);
                if (userFromDb == null)
                {
                    return NotFound();
                }
                userFromDb.Name = updatedUser.Name;
                userFromDb.PhoneNumber = updatedUser.PhoneNumber;

                try
                {
                    _context.Update(userFromDb);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật hồ sơ.");
                }
            }
            return View("Profile", updatedUser);
        }
        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            if (!IsUserLoggedIn(out int? userId) || userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử.";
                return RedirectToAction("Login", "Account");
            }

            var paymentHistory = await _context.Bills
                                        .Where(b => b.UserID == userId.Value && b.EndTime != null)
                                        .Include(b => b.Computer) 
                                        .OrderByDescending(b => b.EndTime) 
                                        .ToListAsync();

            return View(paymentHistory); 
        }
    }
}
