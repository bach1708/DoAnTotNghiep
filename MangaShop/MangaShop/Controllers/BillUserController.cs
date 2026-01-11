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
                .Where(d => d.MaKhachHang == userId.Value)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(bills);
        }

        // ===== CHI TIẾT ĐƠN =====
        public IActionResult BillDetail(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "NvbAccount");

            var donHang = _context.DonHangs
                .FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == userId.Value);

            if (donHang == null)
                return NotFound();

            var chiTiet = _context.ChiTietDonHangs
                .Where(c => c.MaDonHang == id)
                .Include(c => c.MaTruyenNavigation)
                .Include(c => c.MaTapNavigation) // ✅ để view có SoTap
                .ToList();

            var maTruyenTrongDon = chiTiet
                .Select(x => x.MaTruyen)
                .Distinct()
                .ToList();

            var daDanhGiaIds = _context.DanhGias
                .Where(dg => dg.MaKhachHang == userId.Value && maTruyenTrongDon.Contains(dg.MaTruyen))
                .Select(dg => dg.MaTruyen)
                .ToHashSet();

            ViewBag.DonHang = donHang;
            ViewBag.DaDanhGiaIds = daDanhGiaIds;

            return View(chiTiet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "NvbAccount");

            var donHang = _context.DonHangs
                .FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == userId.Value);

            if (donHang == null)
                return NotFound();

            if (donHang.TrangThai != "Chờ xử lý")
            {
                TempData["Error"] = "Đơn hàng đã được xác nhận/đang giao/hoàn thành nên không thể hủy.";
                return RedirectToAction("BillUser");
            }

            donHang.TrangThai = "Huỷ";
            _context.SaveChanges();

            TempData["Success"] = "Bạn đã hủy đơn hàng thành công.";
            return RedirectToAction("BillUser");
        }
    }
}
