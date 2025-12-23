using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MangaShop.Controllers
{
    public class NvbHomeController : Controller
    {
        private readonly MangaShopContext _context;

        // ✅ Inject DbContext
        public NvbHomeController(MangaShopContext context)
        {
            _context = context;
        }

        public IActionResult Home()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        // ================== DANH MỤC ==================
        public IActionResult DanhMuc(int? maTheLoai)
        {
            var theLoai = _context.TheLoais.ToList();

            var truyen = _context.Truyens
                .Where(t => maTheLoai == null || t.MaTheLoai == maTheLoai)
                .ToList();

            ViewBag.TheLoai = theLoai;
            ViewBag.MaTheLoai = maTheLoai;

            return View(truyen);
        }

        public IActionResult LichPhatHanh()
        {
            return View();
        }

        public IActionResult ThanhVien()
        {
            return View();
        }

        public IActionResult TinTuc()
        {
            return View();
        }

        public IActionResult HuongDan()
        {
            return View();
        }

        public IActionResult VeChungToi()
        {
            return View();
        }
    }
}
