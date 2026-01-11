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

            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");
            if (cart == null || !cart.Any())
                return RedirectToAction("GioHang", "Cart");

            var kh = _context.KhachHangs.FirstOrDefault(x => x.MaKhachHang == userId.Value);

            var vm = new CheckoutVM
            {
                Cart = cart,
                HoTen = kh?.HoTen ?? HttpContext.Session.GetString("UserName") ?? "",
                SoDienThoai = kh?.SoDienThoai ?? "",
                DiaChi = kh?.DiaChi ?? "",
                PaymentMethod = "COD"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(CheckoutVM model)
        {
            var maKH = HttpContext.Session.GetInt32("UserId");
            if (maKH == null)
                return RedirectToAction("Login", "NvbAccount");

            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");
            if (cart == null || !cart.Any())
                return RedirectToAction("GioHang", "Cart");

            using var tran = _context.Database.BeginTransaction();
            try
            {
                // cập nhật info KH
                var kh = _context.KhachHangs.FirstOrDefault(x => x.MaKhachHang == maKH.Value);
                if (kh != null)
                {
                    kh.HoTen = model.HoTen?.Trim();
                    kh.SoDienThoai = model.SoDienThoai?.Trim();
                    kh.DiaChi = model.DiaChi?.Trim();
                }

                // TẠO ĐƠN HÀNG (tính tổng từ DB để an toàn)
                // Load toàn bộ tập trong giỏ để tính đúng giá và kiểm kho
                var tapIds = cart.Select(c => c.MaTap).Distinct().ToList();

                var tapsDb = _context.TruyenTaps
                    .Where(t => tapIds.Contains(t.MaTap))
                    .ToList();

                // kiểm thiếu tập
                if (tapsDb.Count != tapIds.Count)
                {
                    TempData["Error"] = "Có tập truyện không tồn tại. Vui lòng đặt lại!";
                    return RedirectToAction("ThanhToan");
                }

                // kiểm kho trước
                foreach (var item in cart)
                {
                    var tap = tapsDb.First(t => t.MaTap == item.MaTap);

                    if (tap.SoLuongTon < item.SoLuong)
                    {
                        TempData["Error"] = $"Tập {tap.SoTap} không đủ hàng. Hiện còn {tap.SoLuongTon}.";
                        return RedirectToAction("ThanhToan");
                    }
                }

                // tính tổng tiền theo giá DB
                double tongTien = 0;
                foreach (var item in cart)
                {
                    var tap = tapsDb.First(t => t.MaTap == item.MaTap);
                    tongTien += tap.Gia * item.SoLuong;
                }

                var donHang = new DonHang
                {
                    MaKhachHang = maKH.Value,
                    NgayDat = DateTime.Now,
                    TongTien = tongTien,
                    TrangThai = "Chờ xử lý",
                    PhuongThucThanhToan = model.PaymentMethod
                };

                _context.DonHangs.Add(donHang);
                _context.SaveChanges(); // để có MaDonHang

                // CHI TIẾT ĐƠN + TRỪ KHO
                foreach (var item in cart)
                {
                    var tap = tapsDb.First(t => t.MaTap == item.MaTap);

                    // trừ kho
                    tap.SoLuongTon -= item.SoLuong;

                    // ✅ lưu chi tiết đơn có MaTap
                    _context.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        MaTruyen = tap.MaTruyen,     // lấy từ tap cho chắc
                        MaTap = tap.MaTap,           // ✅ QUAN TRỌNG: lưu MaTap để đánh giá theo tập
                        SoLuong = item.SoLuong,
                        DonGia = tap.Gia             // lấy giá từ DB
                    });
                }

                _context.SaveChanges();
                tran.Commit();

                HttpContext.Session.Remove("CART");
                return RedirectToAction("Success");
            }
            catch
            {
                tran.Rollback();
                TempData["Error"] = "Có lỗi khi đặt hàng. Vui lòng thử lại!";
                return RedirectToAction("ThanhToan");
            }
        }

        public IActionResult Success() => View();
    }
}
