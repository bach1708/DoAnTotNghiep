using MangaShop.Helpers;
using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MangaShop.Controllers
{
    public class CartController : Controller
    {
        private readonly MangaShopContext _context;

        public CartController(MangaShopContext context)
        {
            _context = context;
        }

        // --- HÀM PHỤ LẤY USER ID TỪ SESSION ---
        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        // --- XEM GIỎ HÀNG ---
        public IActionResult GioHang()
        {
            var userId = GetUserId();
            List<CartItem> model = new List<CartItem>();

            if (userId != null)
            {
                // Trường hợp ĐÃ ĐĂNG NHẬP: Lấy từ SQL
                model = _context.ChiTietGioHangs
                    .Where(ct => ct.MaGioHangNavigation.MaKhachHang == userId)
                    .Select(ct => new CartItem
                    {
                        MaTruyen = ct.MaTruyen,
                        MaTap = ct.MaTap ?? 0,
                        TenTruyen = ct.MaTapNavigation != null
                                    ? $"{ct.MaTruyenNavigation.TenTruyen} (Tập {ct.MaTapNavigation.SoTap})"
                                    : ct.MaTruyenNavigation.TenTruyen,
                        Gia = (double)(ct.MaTapNavigation != null ? ct.MaTapNavigation.Gia : 0),
                        SoLuong = ct.SoLuong,
                        AnhBia = ct.MaTruyenNavigation.AnhBia
                    }).ToList();
            }
            else
            {
                // Trường hợp KHÁCH VÃNG LAI: Lấy từ Session
                model = HttpContext.Session.GetObject<List<CartItem>>("CART") ?? new List<CartItem>();
            }

            return View(model);
        }

        // --- THÊM TẬP VÀO GIỎ (Hỗ trợ cả DB và Session) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTap(int maTap, int quantity)
        {
            if (quantity < 1) quantity = 1;
            var userId = GetUserId();

            var tap = _context.TruyenTaps
                .Include(x => x.MaTruyenNavigation)
                .FirstOrDefault(x => x.MaTap == maTap);

            if (tap == null) return NotFound();
            if (tap.SoLuongTon <= 0)
            {
                TempData["Err"] = "Tập này đã hết hàng.";
                return RedirectToAction("ChonTap", new { id = tap.MaTruyen });
            }

            if (userId != null)
            {
                // LOGIC DATABASE
                var gioHang = _context.GioHangs.FirstOrDefault(g => g.MaKhachHang == userId);
                if (gioHang == null)
                {
                    gioHang = new GioHang { MaKhachHang = userId.Value, NgayTao = System.DateTime.Now };
                    _context.GioHangs.Add(gioHang);
                    _context.SaveChanges();
                }

                var ctgh = _context.ChiTietGioHangs
                    .FirstOrDefault(ct => ct.MaGioHang == gioHang.MaGioHang && ct.MaTap == maTap);

                if (ctgh != null)
                {
                    ctgh.SoLuong = System.Math.Min(ctgh.SoLuong + quantity, tap.SoLuongTon);
                }
                else
                {
                    _context.ChiTietGioHangs.Add(new ChiTietGioHang
                    {
                        MaGioHang = gioHang.MaGioHang,
                        MaTruyen = tap.MaTruyen,
                        MaTap = maTap,
                        SoLuong = System.Math.Min(quantity, tap.SoLuongTon)
                    });
                }
                _context.SaveChanges();
            }
            else
            {
                // LOGIC SESSION
                var cart = HttpContext.Session.GetObject<List<CartItem>>("CART") ?? new List<CartItem>();
                var item = cart.FirstOrDefault(c => c.MaTap == maTap);
                if (item == null)
                {
                    cart.Add(new CartItem
                    {
                        MaTap = tap.MaTap,
                        MaTruyen = tap.MaTruyen,
                        TenTruyen = tap.MaTruyenNavigation.TenTruyen,
                        AnhBia = tap.MaTruyenNavigation.AnhBia,
                        Gia = tap.Gia,
                        SoLuong = System.Math.Min(quantity, tap.SoLuongTon)
                    });
                }
                else
                {
                    item.SoLuong = System.Math.Min(item.SoLuong + quantity, tap.SoLuongTon);
                }
                HttpContext.Session.SetObject("CART", cart);
            }

            return RedirectToAction("GioHang");
        }

        // --- CẬP NHẬT SỐ LƯỢNG (Increase/Decrease/Update) ---
        public IActionResult UpdateQuantityDB(int maTap, int quantity)
        {
            var userId = GetUserId();
            var tap = _context.TruyenTaps.Find(maTap);
            if (tap == null) return NotFound();

            if (userId != null)
            {
                var ctgh = _context.ChiTietGioHangs
                    .FirstOrDefault(ct => ct.MaGioHangNavigation.MaKhachHang == userId && ct.MaTap == maTap);
                if (ctgh != null)
                {
                    if (quantity <= 0) _context.ChiTietGioHangs.Remove(ctgh);
                    else ctgh.SoLuong = System.Math.Min(quantity, tap.SoLuongTon);
                    _context.SaveChanges();
                }
            }
            else
            {
                var cart = HttpContext.Session.GetObject<List<CartItem>>("CART") ?? new List<CartItem>();
                var item = cart.FirstOrDefault(i => i.MaTap == maTap);
                if (item != null)
                {
                    if (quantity <= 0) cart.Remove(item);
                    else item.SoLuong = System.Math.Min(quantity, tap.SoLuongTon);
                    HttpContext.Session.SetObject("CART", cart);
                }
            }
            return RedirectToAction("GioHang");
        }

        public IActionResult Increase(int id)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                var item = _context.ChiTietGioHangs.FirstOrDefault(ct => ct.MaGioHangNavigation.MaKhachHang == userId && ct.MaTap == id);
                return UpdateQuantityDB(id, (item?.SoLuong ?? 0) + 1);
            }
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");
            var sItem = cart?.FirstOrDefault(c => c.MaTap == id);
            return UpdateQuantityDB(id, (sItem?.SoLuong ?? 0) + 1);
        }

        public IActionResult Decrease(int id)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                var item = _context.ChiTietGioHangs.FirstOrDefault(ct => ct.MaGioHangNavigation.MaKhachHang == userId && ct.MaTap == id);
                return UpdateQuantityDB(id, (item?.SoLuong ?? 0) - 1);
            }
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");
            var sItem = cart?.FirstOrDefault(c => c.MaTap == id);
            return UpdateQuantityDB(id, (sItem?.SoLuong ?? 0) - 1);
        }

        public IActionResult Remove(int id)
        {
            return UpdateQuantityDB(id, 0);
        }

        public IActionResult Clear()
        {
            var userId = GetUserId();
            if (userId != null)
            {
                var gioHang = _context.GioHangs.FirstOrDefault(g => g.MaKhachHang == userId);
                if (gioHang != null)
                {
                    var details = _context.ChiTietGioHangs.Where(ct => ct.MaGioHang == gioHang.MaGioHang);
                    _context.ChiTietGioHangs.RemoveRange(details);
                    _context.SaveChanges();
                }
            }
            else
            {
                HttpContext.Session.Remove("CART");
            }
            return RedirectToAction("GioHang");
        }

        public IActionResult ChonTap(int id)
        {
            var truyen = _context.Truyens.Include(t => t.TruyenTaps).FirstOrDefault(t => t.MaTruyen == id);
            if (truyen == null) return NotFound();
            return View(truyen);
        }
    }
}