using MangaShop.Models;
using MangaShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using MangaShop.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly MangaShopContext _context;

        public CheckoutController(MangaShopContext context)
        {
            _context = context;
        }

        public IActionResult ThanhToan()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "NvbAccount");

            var cart = _context.ChiTietGioHangs
                .Include(ct => ct.MaTruyenNavigation)
                .Include(ct => ct.MaTapNavigation)
                .Where(ct => ct.MaGioHangNavigation.MaKhachHang == userId)
                .Select(ct => new CartItem
                {
                    MaTruyen = ct.MaTruyen,
                    MaTap = ct.MaTap ?? 0,
                    TenTruyen = ct.MaTruyenNavigation.TenTruyen,
                    SoTap = ct.MaTapNavigation != null ? ct.MaTapNavigation.SoTap : 0,
                    Gia = (double)(ct.MaTapNavigation != null ? ct.MaTapNavigation.Gia : 0),
                    SoLuong = ct.SoLuong,
                    AnhBia = ct.MaTruyenNavigation.AnhBia
                }).ToList();

            if (!cart.Any())
            {
                cart = HttpContext.Session.GetObject<List<CartItem>>("CART") ?? new List<CartItem>();
            }

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("GioHang", "Cart");
            }

            var kh = _context.KhachHangs.FirstOrDefault(x => x.MaKhachHang == userId.Value);

            var vm = new CheckoutVM
            {
                Cart = cart,
                HoTen = kh?.HoTen ?? HttpContext.Session.GetString("UserName") ?? "",
                SoDienThoai = kh?.SoDienThoai ?? "",
                DiaChi = kh?.DiaChi ?? "",
                PaymentMethod = "COD"
            };

            double tongChiTieu = _context.DonHangs
                .Where(d => d.MaKhachHang == userId.Value && d.TrangThai != "Huỷ" && d.TrangThai != "Đã Hủy")
                .Select(d => (double?)d.TongTien).Sum() ?? 0;

            int giamPhanTram = (tongChiTieu >= 10_000_000) ? 8 : (tongChiTieu >= 5_000_000) ? 5 : (tongChiTieu >= 1_000_000) ? 3 : 0;
            ViewBag.GiamPhanTram = giamPhanTram;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(CheckoutVM model)
        {
            var maKH = HttpContext.Session.GetInt32("UserId");
            if (maKH == null) return RedirectToAction("Login", "NvbAccount");

            var cart = _context.ChiTietGioHangs
                .Include(ct => ct.MaTapNavigation)
                .Where(ct => ct.MaGioHangNavigation.MaKhachHang == maKH.Value)
                .ToList();

            if (cart == null || !cart.Any()) return RedirectToAction("GioHang", "Cart");

            using var tran = _context.Database.BeginTransaction();
            try
            {
                var kh = _context.KhachHangs.FirstOrDefault(x => x.MaKhachHang == maKH.Value);
                if (kh != null)
                {
                    kh.HoTen = model.HoTen?.Trim();
                    kh.SoDienThoai = model.SoDienThoai?.Trim();
                    kh.DiaChi = model.DiaChi?.Trim();
                }

                double tongTienHang = cart.Sum(item => (double)(item.MaTapNavigation?.Gia ?? 0) * item.SoLuong);
                double tongChiTieu = _context.DonHangs
                    .Where(d => d.MaKhachHang == maKH.Value && d.TrangThai != "Huỷ" && d.TrangThai != "Đã Hủy")
                    .Select(d => (double?)d.TongTien).Sum() ?? 0;

                int giamPhanTram = (tongChiTieu >= 10_000_000) ? 8 : (tongChiTieu >= 5_000_000) ? 5 : (tongChiTieu >= 1_000_000) ? 3 : 0;
                double tienGiam = Math.Round(tongTienHang * giamPhanTram / 100.0, 0);
                double phiBaoHiem = model.CoBaoHiem ? Math.Round(tongTienHang * 0.01, 0) : 0;
                double tongThanhToan = tongTienHang - tienGiam + phiBaoHiem;

                var donHang = new DonHang
                {
                    MaKhachHang = maKH.Value,
                    NgayDat = DateTime.Now,
                    TongTien = tongTienHang,
                    GiamGiaPhanTram = giamPhanTram,
                    TienGiam = tienGiam,
                    TongThanhToan = tongThanhToan,
                    TrangThai = "Chờ xử lý",
                    PhuongThucThanhToan = model.PaymentMethod,
                    GhiChu = model.CoBaoHiem ? "Đã mua bảo hiểm sản phẩm (1%)" : ""
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges();

                foreach (var item in cart)
                {
                    item.MaTapNavigation.SoLuongTon -= item.SoLuong;
                    _context.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        MaTruyen = item.MaTruyen,
                        MaTap = item.MaTap,
                        SoLuong = item.SoLuong,
                        DonGia = item.MaTapNavigation.Gia
                    });
                }

                _context.ChiTietGioHangs.RemoveRange(cart);
                _context.SaveChanges();
                tran.Commit();

                // Chuyển sang string để TempData không bị lỗi Serialize Double
                TempData["TotalAmount"] = tongThanhToan.ToString();
                HttpContext.Session.Remove("CART");

                return RedirectToAction("Success");
            }
            catch (Exception)
            {
                tran.Rollback();
                TempData["Error"] = "Lỗi hệ thống khi xử lý đơn hàng.";
                return RedirectToAction("ThanhToan");
            }
        }

        public IActionResult Success()
        {
            // Lấy từ TempData ra một biến string
            string total = TempData["TotalAmount"]?.ToString() ?? "0";

            // Truyền sang View qua ViewBag nhưng dưới dạng chuỗi
            ViewBag.TotalAmount = total;

            return View();
        }
    }
}