using MangaShop.Helpers;
using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace MangaShop.Controllers
{
    public class CartController : Controller
    {
        private readonly MangaShopContext _context;

        public CartController(MangaShopContext context)
        {
            _context = context;
        }

        // LẤY GIỎ HÀNG
        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART");
            if (cart == null)
            {
                cart = new List<CartItem>();
                HttpContext.Session.SetObject("CART", cart);
            }
            return cart;
        }

        // XEM GIỎ HÀNG
        public IActionResult GioHang()
        {
            return View(GetCart());
        }
        // TĂNG SỐ LƯỢNG
        public IActionResult Increase(int id) // id = MaTap
        {
            var tap = _context.TruyenTaps.FirstOrDefault(x => x.MaTap == id);
            if (tap == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MaTap == id);

            if (item != null)
            {
                if (item.SoLuong < tap.SoLuongTon) item.SoLuong++;
            }

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("GioHang");
        }



        // GIẢM SỐ LƯỢNG
        public IActionResult Decrease(int id) // id = MaTap
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MaTap == id);

            if (item != null)
            {
                item.SoLuong--;
                if (item.SoLuong <= 0) cart.Remove(item);
            }

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("GioHang");
        }

        // GET: hiển thị chọn tập
        public IActionResult ChonTap(int id) // id = MaTruyen
        {
            var truyen = _context.Truyens
                .Include(t => t.TruyenTaps)
                .FirstOrDefault(t => t.MaTruyen == id);

            if (truyen == null) return NotFound();
            return View(truyen);
        }

        // POST: thêm tập vào giỏ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTap(int maTap, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var tap = _context.TruyenTaps
                .Include(x => x.MaTruyenNavigation)
                .FirstOrDefault(x => x.MaTap == maTap);

            if (tap == null) return NotFound();

            // ✅ hết hàng
            if (tap.SoLuongTon <= 0)
            {
                TempData["Err"] = "Tập này đã hết hàng.";
                return RedirectToAction("ChonTap", new { id = tap.MaTruyen });
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MaTap == maTap);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    MaTap = tap.MaTap,
                    MaTruyen = tap.MaTruyen,
                    SoTap = tap.SoTap,
                    TenTruyen = tap.MaTruyenNavigation.TenTruyen,
                    AnhBia = tap.MaTruyenNavigation.AnhBia,
                    Gia = tap.Gia,
                    SoLuong = Math.Min(quantity, tap.SoLuongTon) // ✅ không vượt tồn
                });
            }
            else
            {
                int next = item.SoLuong + quantity;
                if (next > tap.SoLuongTon) next = tap.SoLuongTon; // ✅ không vượt tồn
                item.SoLuong = next;
            }

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("GioHang");
        }


        // THÊM VÀO GIỎ
        public IActionResult Add(int id) // id = MaTruyen
        {
            return RedirectToAction("ChonTap", new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int id, int quantity) // id = MaTap
        {
            if (quantity < 1) quantity = 1;

            var tap = _context.TruyenTaps.FirstOrDefault(x => x.MaTap == id);
            if (tap == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MaTap == id);

            if (item != null)
            {
                if (quantity > tap.SoLuongTon) quantity = tap.SoLuongTon; // ✅ chặn vượt tồn
                item.SoLuong = quantity;
                HttpContext.Session.SetObject("CART", cart);
            }

            return RedirectToAction("GioHang");
        }



        // XÓA 1 SẢN PHẨM
        public IActionResult Remove(int id) // id = MaTap
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.MaTap == id);

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("GioHang");
        }


        // XÓA HẾT
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("CART");
            return RedirectToAction("GioHang");
        }

    }
}
