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

        public IActionResult BillAdmin(int page = 1)
        {
            int pageSize = 10; // Số lượng đơn hàng trên mỗi trang

            var totalBills = _context.DonHangs.Count(); // Tổng số đơn hàng

            var bills = _context.DonHangs
                .Include(d => d.MaKhachHangNavigation)
                .OrderByDescending(d => d.NgayDat)
                .Skip((page - 1) * pageSize) // Bỏ qua các đơn hàng của trang trước
                .Take(pageSize)              // Lấy đúng số lượng của trang hiện tại
                .ToList();

            // Truyền thông tin phân trang qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBills / pageSize);

            return View(bills);
        }

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
                .Include(c => c.MaTapNavigation) // ✅ thêm
                .ToList();

            ViewBag.DonHang = donHang;
            return View(chiTiet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Thêm để bảo mật chống giả mạo request
        public IActionResult UpdateStatus(int id, string trangThai)
        {
            var donHang = _context.DonHangs.Find(id);

            if (donHang != null)
            {
                // 1. Kiểm tra nếu trạng thái hiện tại đã là cuối cùng (Huỷ hoặc Hoàn thành)
                if (donHang.TrangThai == "Huỷ" || donHang.TrangThai == "Hoàn thành")
                {
                    // Có thể thêm thông báo lỗi vào TempData để hiển thị ở View nếu cần
                    TempData["Error"] = "Đơn hàng đã đóng, không thể thay đổi trạng thái.";
                    return RedirectToAction("BillAdmin");
                }

                // 2. Nếu trạng thái mới là "Huỷ", bạn có thể thêm logic hoàn kho ở đây (nếu cần)

                // 3. Cập nhật trạng thái mới
                donHang.TrangThai = trangThai;
                _context.SaveChanges();

                TempData["Success"] = "Cập nhật trạng thái thành công!";
            }

            return RedirectToAction("BillAdmin");
        }
    }
}
