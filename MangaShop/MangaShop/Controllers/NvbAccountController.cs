using Microsoft.AspNetCore.Mvc;
using MangaShop.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace MangaShop.Controllers
{
    public class NvbAccountController : Controller
    {
        private readonly MangaShopContext _context;

        public NvbAccountController(MangaShopContext context)
        {
            _context = context;
        }

        // ===== GET: Mở form đăng nhập USER =====
        [HttpGet]
        public IActionResult Login()
        {
            return View("NvbUserLogin");
        }

        // ===== POST: Xử lý đăng nhập USER =====
        [HttpPost]
        public IActionResult Login(string Email, string MatKhau)
        {
            var user = _context.KhachHangs
                .FirstOrDefault(u => u.Email == Email && u.MatKhau == MatKhau);

            if (user != null)
            {
                // Lưu session user
                HttpContext.Session.SetInt32("UserId", user.MaKhachHang);
                HttpContext.Session.SetString("UserName", user.HoTen);

                return RedirectToAction("Home", "NvbHome");
            }

            ViewBag.Error = "Email hoặc mật khẩu không đúng";
            return View("NvbUserLogin");
        }
        // ===== GET: Mở form đăng ký =====
        [HttpGet]
        public IActionResult Register()
        {
            return View("NvbUserRegister");
        }

        // ===== POST: Xử lý đăng ký =====
        [HttpPost]
        public IActionResult Register(KhachHang kh)
        {
            // Kiểm tra email đã tồn tại chưa
            if (_context.KhachHangs.Any(u => u.Email == kh.Email))
            {
                ViewBag.Error = "Email đã tồn tại";
                return View("NvbUserRegister");
            }

            kh.NgayTao = DateTime.Now;

            _context.KhachHangs.Add(kh);
            _context.SaveChanges();

            // Sau khi đăng ký xong → quay về Login
            return RedirectToAction("Login");
        }


        // ===== Đăng xuất USER =====
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserName");

            return RedirectToAction("Login");
        }
    }
}
