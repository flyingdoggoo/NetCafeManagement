using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNet.Data;
using QuanLyNet.Models;

namespace QuanLyNet.Controllers.Account
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminUser()
        {
            return HttpContext.Session.GetString(AccountController.SessionKeyIsAdmin) == "True";
        }

        public IActionResult Index()
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "Account"); // Hoặc trang chủ
            }
            ViewBag.Message = "Đây là trang quản lý của Admin.";
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "Account");
            }
            var users = await _context.Users
                .Where(u => u.Username != "Admin")
                .ToListAsync();
            return View(users);
        }
        [HttpGet]
        public async Task<IActionResult> ComputerManagement()
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "Account");
            }
            var computers = await _context.Computers
                .OrderBy(b => b.Name)
                .ToListAsync();
            return View(computers);
        }
        [HttpPost]
        public IActionResult AddComputer()
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "Account");
            }
            return View(new Computer());
        }
        [HttpPost]
        public async Task<IActionResult> AddComputer(Computer computer)
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "Account");
            }
            bool nameExists = await _context.Computers.AnyAsync(c => c.Name == computer.Name);
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"Tên máy '{computer.Name}' đã tồn tại.");
            }
            ModelState.Remove(nameof(computer.ID));
            ModelState.Remove(nameof(computer.Bills));
            if (ModelState.IsValid)
            {
                try
                {
                    computer.Status = ComputerStatus.Available;

                    _context.Computers.Add(computer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã thêm thành công máy {computer.Name}.";
                    return RedirectToAction(nameof(ComputerManagement));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm máy mới.");
                    return View(computer);
                }
            }
            return View(computer);
        }
        [HttpGet]
        public async Task<IActionResult> EditComputer(int id)
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "Account");
            }
            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy máy tính.";
                return RedirectToAction(nameof(ComputerManagement));
            }
            return View(computer);
        }
        [HttpPost]
        public async Task<IActionResult> EditComputer(int id, Computer computer)
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "Account");
            }
            if (id != computer.ID)
            {
                TempData["ErrorMessage"] = "ID máy tính không hợp lệ.";
                return RedirectToAction(nameof(ComputerManagement));
            }
            bool nameExists = await _context.Computers.AnyAsync(c => c.Name == computer.Name && c.ID != computer.ID);
            if (nameExists)
            {
                ModelState.AddModelError("Name", $"Tên máy '{computer.Name}' đã được sử dụng bởi máy khác.");
            }
            var existingComputer = await _context.Computers.AsNoTracking().FirstOrDefaultAsync(c => c.ID == id);
            if (existingComputer == null) { return NotFound(); }

            if (existingComputer.Status == ComputerStatus.InUse && computer.Status != ComputerStatus.InUse)
            {
                ModelState.AddModelError("Status", "Không thể thay đổi trạng thái máy đang được sử dụng.");
            }
            ModelState.Remove(nameof(computer.Bills));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(computer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã cập nhật thành công máy {computer.Name}.";
                    return RedirectToAction(nameof(ComputerManagement));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu. Máy này có thể đã được cập nhật.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật máy.");
                }
            }
            return View(computer);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteComputer(int id)
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "Account");
            }

            var computerToDelete = await _context.Computers.FindAsync(id);

            if (computerToDelete == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy máy tính để xóa.";
                return RedirectToAction(nameof(ComputerManagement));
            }

            if (computerToDelete.Status == ComputerStatus.InUse)
            {
                TempData["ErrorMessage"] = $"Máy {computerToDelete.Name} đang được sử dụng, không thể xóa.";
                return RedirectToAction(nameof(ComputerManagement));
            }
            _context.Computers.Remove(computerToDelete);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa thành công máy {computerToDelete.Name}.";

            return RedirectToAction(nameof(ComputerManagement));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction("Login", "Account");
            }
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                bool hasActiveSession = await _context.Bills.AnyAsync(b => b.UserID == userId && b.EndTime == null);
                if (hasActiveSession)
                {
                    TempData["ErrorMessage"] = $"Người dùng {user.Username} đang sử dụng máy, không thể xóa.";
                    return RedirectToAction(nameof(UserManagement));
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Người dùng đã được xóa thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
            }
            return RedirectToAction(nameof(UserManagement));
        }
        [HttpPost]
        public async Task<IActionResult> AddBalance(int userId, decimal amount)
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction("Login", "Account");
            }
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Balance += amount;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm {amount} vào tài khoản của {user.Username}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
            }
            return RedirectToAction(nameof(UserManagement));
        }
        [HttpGet]
        public async Task<IActionResult> BillManagement(DateTime? startDate, DateTime? endDate)
        {
            if (!IsAdminUser())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này.";
                return RedirectToAction("Login", "Account");
            }

            DateTime start = startDate ?? DateTime.MinValue;
            DateTime end = endDate ?? DateTime.MaxValue;
            if (endDate.HasValue)
            {
                end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }
            var query = _context.Bills.AsQueryable();

            query = query.Where(b => b.EndTime != null);
            query = query.Where(b => b.EndTime.Value >= start && b.EndTime.Value <= end);

            var bills = await query
                                .Include(b => b.User)
                                .Include(b => b.Computer)
                                .OrderByDescending(b => b.EndTime)
                                .ToListAsync();
            decimal totalRevenue = (decimal)bills.Sum(b => b.TotalCost);

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.StartDateFilter = startDate;
            ViewBag.EndDateFilter = endDate;

            return View(bills);
        }

    }
}
