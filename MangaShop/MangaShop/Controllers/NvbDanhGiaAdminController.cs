using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class NvbDanhGiaAdminController : BaseAdminController
    {
        private readonly MangaShopContext _context;

        public NvbDanhGiaAdminController(MangaShopContext context)
        {
            _context = context;
        }

        // ✅ Danh sách đánh giá + lọc cơ bản
        public IActionResult Index(int? maTruyen, int? soSao)
        {
            var query = _context.DanhGias
                .Include(d => d.MaTruyenNavigation)
                .Include(d => d.MaKhachHangNavigation)
                .AsQueryable();

            if (maTruyen != null)
                query = query.Where(d => d.MaTruyen == maTruyen);

            if (soSao != null)
                query = query.Where(d => d.SoSao == soSao);

            var data = query
                .OrderByDescending(d => d.NgayDanhGia)
                .ToList();

            ViewBag.Truyens = _context.Truyens
                .Select(t => new { t.MaTruyen, t.TenTruyen })
                .ToList();

            ViewBag.MaTruyen = maTruyen;
            ViewBag.SoSao = soSao;

            return View(data);
        }

        // ✅ Xóa đánh giá
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var dg = _context.DanhGias.Find(id);
            if (dg != null)
            {
                _context.DanhGias.Remove(dg);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
