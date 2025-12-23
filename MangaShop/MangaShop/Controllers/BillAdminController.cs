using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class BillAdminController : BaseAdminController
    {
        private readonly MangaShopContext _context;

        public BillAdminController(MangaShopContext context)
        {
            _context = context;
        }

        // ===== DANH SÁCH ĐƠN HÀNG (ADMIN) =====
        public IActionResult BillAdmin()
        {
            var bills = _context.DonHangs
                .Include(d => d.MaKhachHangNavigation)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(bills);
        }

        // ===== CHI TIẾT ĐƠN HÀNG =====
        public IActionResult BillDetailAdmin(int id)
        {
            var donHang = _context.DonHangs
                .Include(d => d.MaKhachHangNavigation)
                .FirstOrDefault(d => d.MaDonHang == id);

            if (donHang == null)
                return NotFound();

            var chiTiet = _context.ChiTietDonHangs
                .Where(c => c.MaDonHang == id)
                .Include(c => c.MaTruyenNavigation)
                .ToList();

            ViewBag.DonHang = donHang;
            return View(chiTiet);
        }

        // ===== DUYỆT / CẬP NHẬT TRẠNG THÁI =====
        [HttpPost]
        public IActionResult UpdateStatus(int id, string trangThai)
        {
            var donHang = _context.DonHangs.Find(id);
            if (donHang != null)
            {
                donHang.TrangThai = trangThai;
                _context.SaveChanges();
            }

            return RedirectToAction("BillAdmin");
        }
    }
}
