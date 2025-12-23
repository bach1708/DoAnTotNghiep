using Microsoft.AspNetCore.Mvc;
using MangaShop.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

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
        public IActionResult Login()
        {
            return View("NvbAdminLogin");
        }

        [HttpPost]
        public IActionResult Login(string TaiKhoan, string MatKhau)
        {
            var admin = _context.QuanTris
                .FirstOrDefault(a => a.TaiKhoan == TaiKhoan
                                  && a.MatKhau == MatKhau
                                  && a.TrangThai == true);

            if (admin != null)
            {
                // ❌ XÓA SESSION USER (nếu có)
                HttpContext.Session.Remove("UserId");
                HttpContext.Session.Remove("UserName");

                // ✅ SET SESSION ADMIN
                HttpContext.Session.SetString("AdminLogin", admin.TaiKhoan);

                return RedirectToAction("Index");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng";
            return View("NvbAdminLogin");
        }


        // ====== Trang Dashboard Admin ======
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login");
            }
            return View("NvbHomeAdmin");
        }

        // ====== Đăng xuất ======
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLogin"); // chỉ xóa admin
            return RedirectToAction("Login");          // quay về login admin
        }
    }
}
