using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MangaShop.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class QuanLyKhachHangController : BaseAdminController
    {
        private readonly MangaShopContext _context;

        public QuanLyKhachHangController(MangaShopContext context)
        {
            _context = context;
        }

        // ✅ Danh sách khách hàng
        public IActionResult Index(string? keyword)
        {
            var query = _context.KhachHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(k =>
                    k.HoTen.Contains(keyword) ||
                    k.Email.Contains(keyword) ||
                    k.SoDienThoai.Contains(keyword));
            }

            var data = query
                .OrderByDescending(k => k.NgayTao)
                .ToList();

            // ✅ Tính tổng chi tiêu theo khách (chỉ tính đơn Hoàn thành)
            var ids = data.Select(x => x.MaKhachHang).ToList();

            var chiTieuDict = _context.DonHangs
                .Where(d => ids.Contains(d.MaKhachHang) && d.TrangThai == "Hoàn thành")
                .GroupBy(d => d.MaKhachHang)
                .Select(g => new
                {
                    MaKhachHang = g.Key,
                    TongChiTieu = g.Sum(x => (double)x.TongTien)
                })
                .ToDictionary(x => x.MaKhachHang, x => x.TongChiTieu);

            // ✅ Tính hạng từ tổng chi tiêu
            var hangDict = ids.ToDictionary(
                id => id,
                id =>
                {
                    var tong = chiTieuDict.ContainsKey(id) ? chiTieuDict[id] : 0;
                    return MemberTierHelper.GetTier(tong);
                });

            ViewBag.Keyword = keyword;
            ViewBag.ChiTieuDict = chiTieuDict; // Dictionary<int,double>
            ViewBag.HangDict = hangDict;       // Dictionary<int,string>

            return View(data);
        }


        // ✅ Chi tiết khách hàng
        public IActionResult Details(int id)
        {
            var kh = _context.KhachHangs
                .Include(k => k.DonHangs)
                .FirstOrDefault(k => k.MaKhachHang == id);

            if (kh == null) return NotFound();

            double tongChiTieu = _context.DonHangs
                .Where(d => d.MaKhachHang == id && d.TrangThai == "Hoàn thành")
                .Sum(d => (double?)d.TongTien) ?? 0;

            ViewBag.TongChiTieu = tongChiTieu;
            ViewBag.Hang = MemberTierHelper.GetTier(tongChiTieu);

            return View(kh);
        }

    }
}
