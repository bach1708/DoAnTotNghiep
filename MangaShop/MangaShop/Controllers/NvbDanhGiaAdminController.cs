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
            // Lấy danh sách đánh giá bao gồm thông tin Truyện, Khách hàng và TẬP
            var query = _context.DanhGias
                .Include(d => d.MaTruyenNavigation)
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaTapNavigation) // Quan trọng: Thêm dòng này
                .AsQueryable();

            // ... các đoạn lọc (Filter) theo maTruyen, soSao giữ nguyên ...

            var model = query.OrderByDescending(x => x.NgayDanhGia).ToList();

            // Đổ dữ liệu truyện ra ViewBag cho dropdown lọc
            ViewBag.Truyens = _context.Truyens.ToList();
            ViewBag.MaTruyen = maTruyen;
            ViewBag.SoSao = soSao;

            return View(model);
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
