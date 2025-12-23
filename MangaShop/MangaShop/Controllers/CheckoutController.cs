using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using MangaShop.Helpers;
namespace MangaShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly MangaShopContext _context;

        public CheckoutController(MangaShopContext context)
        {
            _context = context;
        }

        // KIỂM TRA TRƯỚC KHI THANH TOÁN
        public IActionResult ThanhToan()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "NvbAccount");
            }

            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("GioHang", "Cart");
            }

            return View(cart);
        }

        // XÁC NHẬN THANH TOÁN
        [HttpPost]
        public IActionResult Confirm()
        {
            var maKH = HttpContext.Session.GetInt32("UserId");
            if (maKH == null)
                return RedirectToAction("Login", "NvbAccount");

            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");
            if (cart == null || !cart.Any())
                return RedirectToAction("GioHang", "Cart");

            // TẠO ĐƠN HÀNG
            var donHang = new DonHang
            {
                MaKhachHang = maKH.Value,
                NgayDat = DateTime.Now,
                TongTien = cart.Sum(c => c.ThanhTien)
            };

            _context.DonHangs.Add(donHang);
            _context.SaveChanges();

            // CHI TIẾT ĐƠN
            foreach (var item in cart)
            {
                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaTruyen = item.MaTruyen,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia
                });
            }

            _context.SaveChanges();
            HttpContext.Session.Remove("CART");

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
