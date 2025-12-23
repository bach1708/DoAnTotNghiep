using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class BillController : Controller
    {
        private readonly MangaShopContext _context;

        public BillController(MangaShopContext context)
        {
            _context = context;
        }

        // ===== BILL USER =====
        public IActionResult BillUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "NvbAccount");

            var bills = _context.DonHangs
                .Where(d => d.MaKhachHang == userId)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(bills);
        }

        // ===== CHI TIẾT ĐƠN =====
        public IActionResult BillDetail(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var donHang = _context.DonHangs
                .FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == userId);

            if (donHang == null)
                return NotFound();

            var chiTiet = _context.ChiTietDonHangs
                .Where(c => c.MaDonHang == id)
                .Include(c => c.MaTruyenNavigation)
                .ToList();

            ViewBag.DonHang = donHang;
            return View(chiTiet);
        }
    }
}
