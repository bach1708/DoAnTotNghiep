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
        public IActionResult BillUser(int page = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "NvbAccount");

            int pageSize = 10;
            var query = _context.DonHangs
                .Where(d => d.MaKhachHang == userId)
                .OrderByDescending(d => d.NgayDat);

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Đảm bảo trang hiện tại hợp lệ
            if (page < 1) page = 1;

            var result = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(result);
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

        // 1. Trang hiện lý do hủy (GET)
        // 1. Trang hiện lý do hủy (GET)
        [HttpGet]
        public IActionResult Cancel(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "NvbAccount");

            var donHang = _context.DonHangs
                .FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == userId.Value);

            if (donHang == null) return NotFound();

            // Chỉ cho phép hủy nếu đang chờ xử lý
            if (donHang.TrangThai != "Chờ xử lý")
            {
                TempData["Error"] = "Đơn hàng không thể hủy ở trạng thái này.";
                return RedirectToAction("BillUser");
            }

            // SỬA TẠI ĐÂY: Chỉ định rõ ràng tên View là "BillCancer"
            return View("BillCancer", donHang);
        }

        // 2. Xử lý lưu lý do và cập nhật trạng thái (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmCancel(int id, string reason, string? otherReason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "NvbAccount");

            var donHang = _context.DonHangs
                .FirstOrDefault(d => d.MaDonHang == id && d.MaKhachHang == userId.Value);

            if (donHang == null) return NotFound();

            if (donHang.TrangThai == "Chờ xử lý")
            {
                donHang.TrangThai = "Đã huỷ";

                // Logic chọn lý do
                string finalReason = reason == "Khác" ? (otherReason ?? "Lý do khác") : reason;
                donHang.LyDoHuy = finalReason; // Lưu vào cột LyDoHuy bạn vừa thêm vào Model

                _context.SaveChanges();
                TempData["Success"] = "Bạn đã hủy đơn hàng thành công.";
            }

            return RedirectToAction("BillUser");
        }
    }
}
