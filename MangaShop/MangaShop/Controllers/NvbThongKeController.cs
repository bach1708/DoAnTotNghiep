using MangaShop.Models;
using MangaShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class NvbThongKeController : BaseAdminController
    {
        private readonly MangaShopContext _context;

        public NvbThongKeController(MangaShopContext context)
        {
            _context = context;
        }

        public IActionResult Index(DateTime? tuNgay, DateTime? denNgay)
        {
            // mặc định 30 ngày gần nhất
            DateTime end = (denNgay ?? DateTime.Today).Date.AddDays(1).AddTicks(-1); // hết ngày
            DateTime start = (tuNgay ?? DateTime.Today.AddDays(-29)).Date;           // đầu ngày

            // lọc đơn theo khoảng ngày
            var donTrongKhoang = _context.DonHangs
                .Where(d => d.NgayDat >= start && d.NgayDat <= end);

            int tongDon = donTrongKhoang.Count();
            int donHoanThanh = donTrongKhoang.Count(d => d.TrangThai == "Hoàn thành");
            int donHuy = donTrongKhoang.Count(d => d.TrangThai == "Huỷ" || d.TrangThai == "Hủy");

            // doanh thu chỉ tính đơn hoàn thành
            double tongDoanhThu = donTrongKhoang
                .Where(d => d.TrangThai == "Hoàn thành")
                .Sum(d => (double?)d.TongTien) ?? 0;

            // doanh thu theo ngày (đơn hoàn thành)
            var doanhThuTheoNgay = donTrongKhoang
                .Where(d => d.TrangThai == "Hoàn thành")
                .GroupBy(d => d.NgayDat!.Value.Date)
                .Select(g => new DoanhThuTheoNgayVM
                {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(x => (double)x.TongTien),
                    SoDon = g.Count()
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            // Top truyện bán chạy (đơn hoàn thành)
            var topTruyen = _context.ChiTietDonHangs
                .Include(ct => ct.MaTruyenNavigation)
                .Include(ct => ct.MaDonHangNavigation)
                .Where(ct =>
                    ct.MaDonHangNavigation.NgayDat >= start &&
                    ct.MaDonHangNavigation.NgayDat <= end &&
                    ct.MaDonHangNavigation.TrangThai == "Hoàn thành"
                )
                .GroupBy(ct => new { ct.MaTruyen, ct.MaTruyenNavigation.TenTruyen })
                .Select(g => new TopTruyenVM
                {
                    MaTruyen = g.Key.MaTruyen,
                    TenTruyen = g.Key.TenTruyen,
                    TongSoLuong = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.DonGia * x.SoLuong)
                })
                .OrderByDescending(x => x.DoanhThu)
                .Take(10)
                .ToList();

            var vm = new ThongKeVM
            {
                TuNgay = start,
                DenNgay = end.Date,
                TongDoanhThu = tongDoanhThu,
                TongDon = tongDon,
                DonHoanThanh = donHoanThanh,
                DonHuy = donHuy,
                DoanhThuTheoNgay = doanhThuTheoNgay,
                TopTruyen = topTruyen
            };

            return View(vm);
        }
    }
}
