using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using MangaShop.Helpers;
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
        public IActionResult Increase(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MaTruyen == id);

            if (item != null)
            {
                item.SoLuong++;
            }

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("GioHang");
        }

        // GIẢM SỐ LƯỢNG
        public IActionResult Decrease(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MaTruyen == id);

            if (item != null)
            {
                item.SoLuong--;
                if (item.SoLuong <= 0)
                {
                    cart.Remove(item);
                }
            }

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("GioHang");
        }

        // THÊM VÀO GIỎ
        public IActionResult Add(int id)
        {
            var truyen = _context.Truyens.FirstOrDefault(t => t.MaTruyen == id);
            if (truyen == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.MaTruyen == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    MaTruyen = truyen.MaTruyen,
                    TenTruyen = truyen.TenTruyen,
                    AnhBia = truyen.AnhBia,
                    Gia = truyen.Gia,
                    SoLuong = 1
                });
            }
            else
            {
                item.SoLuong++;
            }

            HttpContext.Session.SetObject("CART", cart);
            return RedirectToAction("GioHang");
        }

        // XÓA 1 SẢN PHẨM
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.MaTruyen == id);
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
