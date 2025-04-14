using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNet.Controllers.Account;
using QuanLyNet.Data;
using QuanLyNet.Models;

namespace QuanLyNet.Controllers
{
    public class BillController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BillController(ApplicationDbContext context)
        {
            _context = context;
        }
        private bool IsUserLoggedIn(out int? userId)
        {
            userId = HttpContext.Session.GetInt32(AccountController.SessionKeyUserId);
            return userId != null;
        }
        [HttpGet]
        public async Task<IActionResult> SelectComputer()
        {
            if (!IsUserLoggedIn(out int? userId) || userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để chọn máy.";
                return RedirectToAction("Login", "Account");
            }
            var currentSessions = await _context.Bills
                .Include(b => b.Computer)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.UserID == userId.Value && b.EndTime == null);

            ViewBag.CurrentSession = currentSessions;

            var computers = await _context.Computers.OrderBy(c => c.Name).ToListAsync();
            return View(computers);
        }
        [HttpPost]
        public async Task<IActionResult> StartUsing(int computerId)
        {
            if (!IsUserLoggedIn(out int? userId) || userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để chọn máy.";
                return RedirectToAction("Login", "Account");
            }
            var currentSessions = await _context.Bills
                .Include(b => b.Computer)
                .FirstOrDefaultAsync(b => b.UserID == userId.Value && b.EndTime == null);
            if(currentSessions != null)
            {
                TempData["ErrorMessage"] = "Bạn đang sử dụng một máy khác";
                return RedirectToAction("SelectComputer");
            }
            else
            {
                var user = await _context.Users.FindAsync(userId.Value);
                var computer = await _context.Computers.FindAsync(computerId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                    return RedirectToAction("SelectComputer");
                }
                if(user.Balance <= 0)
                {
                    TempData["ErrorMessage"] = "Số dư không đủ để sử dụng máy này.";
                    return RedirectToAction("SelectComputer");
                }
                if(computer.Status == Models.ComputerStatus.InUse)
                {
                    TempData["ErrorMessage"] = "Máy này đang được sử dụng.";
                    return RedirectToAction("SelectComputer");
                }
                if(computer.Status == Models.ComputerStatus.UnderMaintenance)
                {
                    TempData["ErrorMessage"] = "Máy này đang bảo trì.";
                    return RedirectToAction("SelectComputer");
                }
                computer.Status = Models.ComputerStatus.InUse;
                var newBill = new Models.Bill
                {
                    UserID = userId.Value,
                    ComputerID = computerId,
                    StartTime = DateTime.Now,
                };
                _context.Bills.Add(newBill);
                _context.Update(computer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bắt đầu sử dụng máy {computer.Name}";

                return RedirectToAction("SelectComputer");
            }
        }
        [HttpPost]
        public async Task<IActionResult> StopUsing()
        {
            if (!IsUserLoggedIn(out int? userId) || userId == null)
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn.";
                return RedirectToAction("Login", "Account");
            }
            var currentSession = await _context.Bills
                .Include(b => b.Computer)
                .FirstOrDefaultAsync(b => b.UserID == userId.Value && b.EndTime == null);
            if (currentSession == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiên đang chạy để kết thúc.";
                return RedirectToAction(nameof(SelectComputer));
            }
            var user = await _context.Users.FindAsync(userId.Value);
            var computer = currentSession.Computer;
            if (user == null || computer == null)
            {
                TempData["ErrorMessage"] = "Lỗi không tìm thấy User hoặc Computer liên quan.";
                return RedirectToAction(nameof(SelectComputer));
            }
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - currentSession.StartTime;
            decimal cost = (decimal)duration.TotalHours * computer.Price;
            currentSession.EndTime = endTime;
            currentSession.TotalCost = cost;
            computer.Status = ComputerStatus.Available;
            user.Balance -= cost;
            if (user.Balance < 0)
                user.Balance = 0;
            _context.Bills.Update(currentSession);
            _context.Computers.Update(computer);
            _context.Users.Update(user); 
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString(AccountController.SessionKeyUserBalance, user.Balance.ToString());
            TempData["SuccessMessage"] = $"Đã kết thúc phiên sử dụng máy {computer.Name}. Tổng chi phí: {cost.ToString("N0")} đ. Số dư còn lại: {user.Balance.ToString("N0")} đ.";
            return RedirectToAction(nameof(SelectComputer));
        }
    }
}
