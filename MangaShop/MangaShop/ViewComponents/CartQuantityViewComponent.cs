using MangaShop.Helpers;
using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace MangaShop.ViewComponents
{
    public class CartQuantityViewComponent : ViewComponent
    {
        // 1. Khai báo biến context
        private readonly MangaShopContext _context;

        // 2. Tạo hàm khởi tạo (Constructor) để tiêm context vào
        public CartQuantityViewComponent(MangaShopContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            int total = 0;

            if (userId != null)
            {
                // Đã đăng nhập: Đếm tổng số lượng trong bảng ChiTietGioHang của User này
                total = _context.ChiTietGioHangs
                    .Where(ct => ct.MaGioHangNavigation.MaKhachHang == userId)
                    .Sum(ct => (int?)ct.SoLuong) ?? 0;
            }
            else
            {
                // Chưa đăng nhập: Đếm trong Session
                var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");
                if (cart != null)
                {
                    total = cart.Sum(i => i.SoLuong);
                }
            }

            return View(total);
        }
    }
}