using Microsoft.AspNetCore.Mvc;
using MangaShop.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MangaShop.Models.ViewModels;
using System;
using System.Threading.Tasks;

namespace MangaShop.Controllers
{
    public class NvbAdminController : Controller
    {
        private readonly MangaShopContext _context;

        public NvbAdminController(MangaShopContext context)
        {
            _context = context;
        }

        // ====== Trang đăng nhập Admin ======
        [HttpGet]
        public IActionResult Login() => View("NvbAdminLogin");

        [HttpPost]
        public IActionResult Login(string TaiKhoan, string MatKhau)
        {
            var admin = _context.QuanTris.FirstOrDefault(a => a.TaiKhoan == TaiKhoan && a.MatKhau == MatKhau && a.TrangThai == true);
            if (admin != null)
            {
                HttpContext.Session.Remove("UserId");
                HttpContext.Session.Remove("UserName");
                HttpContext.Session.SetString("AdminLogin", admin.TaiKhoan);
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng";
            return View("NvbAdminLogin");
        }

        // ====== Trang Dashboard Admin (Đã sửa khớp với ViewModel của bạn) ======
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login");
            }

            // 1. Xử lý ngày tháng lọc
            DateTime start = (tuNgay ?? DateTime.Today.AddDays(-29)).Date;
            DateTime end = (denNgay ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);

            // 2. Lấy danh sách đơn hàng trong khoảng thời gian
            var queryDonHang = _context.DonHangs
                .Where(d => d.NgayDat >= start && d.NgayDat <= end);

            // 3. Khởi tạo ViewModel và tính toán các con số
            var model = new ThongKeVM
            {
                TuNgay = start,
                DenNgay = end.Date, // Chỉ lấy phần ngày để hiển thị
                TongDon = await queryDonHang.CountAsync(),
                DonHoanThanh = await queryDonHang.CountAsync(d => d.TrangThai == "Hoàn thành"),
                DonHuy = await queryDonHang.CountAsync(d => d.TrangThai == "Huỷ" || d.TrangThai == "Hủy"),

                // Ép kiểu về double để khớp với ThongKeVM của bạn
                TongDoanhThu = (double)(await queryDonHang
                    .Where(d => d.TrangThai == "Hoàn thành")
                    .SumAsync(d => (double?)d.TongTien) ?? 0),

                // Doanh thu theo ngày (Sử dụng DoanhThuTheoNgayVM)
                DoanhThuTheoNgay = await queryDonHang
                    .Where(d => d.TrangThai == "Hoàn thành")
                    .GroupBy(d => d.NgayDat.Value.Date)
                    .Select(g => new DoanhThuTheoNgayVM
                    {
                        Ngay = g.Key,
                        SoDon = g.Count(),
                        DoanhThu = (double)g.Sum(d => d.TongTien)
                    })
                    .OrderBy(x => x.Ngay)
                    .ToListAsync(),

                // Top truyện bán chạy (Sử dụng TopTruyenVM)
                TopTruyen = await _context.ChiTietDonHangs
                    .Include(ct => ct.MaTruyenNavigation)
                    .Include(ct => ct.MaDonHangNavigation)
                    .Where(ct => ct.MaDonHangNavigation.NgayDat >= start &&
                                 ct.MaDonHangNavigation.NgayDat <= end &&
                                 ct.MaDonHangNavigation.TrangThai == "Hoàn thành")
                    .GroupBy(ct => new { ct.MaTruyen, ct.MaTruyenNavigation.TenTruyen })
                    .Select(g => new TopTruyenVM
                    {
                        MaTruyen = g.Key.MaTruyen,
                        TenTruyen = g.Key.TenTruyen ?? "Không rõ",
                        TongSoLuong = g.Sum(ct => ct.SoLuong),
                        // Tính doanh thu = SoLuong * DonGia và ép kiểu double
                        DoanhThu = (double)g.Sum(ct => ct.SoLuong * ct.DonGia)
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(5)
                    .ToListAsync()
            };

            return View("NvbHomeAdmin", model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLogin");
            return RedirectToAction("Login");
        }
    }
}