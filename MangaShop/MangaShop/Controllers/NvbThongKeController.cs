using Microsoft.AspNetCore.Mvc;
using MangaShop.Models;
using MangaShop.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

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
            // 1. Thiết lập khoảng thời gian lọc (mặc định 30 ngày gần nhất)
            DateTime end = (denNgay ?? DateTime.Today).Date.AddDays(1).AddTicks(-1); // Hết ngày đã chọn
            DateTime start = (tuNgay ?? DateTime.Today.AddDays(-29)).Date;           // Đầu ngày bắt đầu

            // 2. Lấy danh sách đơn hàng trong khoảng thời gian
            var donTrongKhoang = _context.DonHangs
                .Where(d => d.NgayDat >= start && d.NgayDat <= end);

            // 3. Tính toán các chỉ số cơ bản
            int tongDon = donTrongKhoang.Count();
            int donHoanThanh = donTrongKhoang.Count(d => d.TrangThai == "Hoàn thành");
            int donHuy = donTrongKhoang.Count(d => d.TrangThai == "Huỷ" || d.TrangThai == "Hủy");

            // 4. Tính tổng doanh thu (Chỉ tính đơn Hoàn thành)
            // Ép kiểu (double?) để tránh lỗi Sum trên tập dữ liệu rỗng và khớp với kiểu double của bạn
            double tongDoanhThu = donTrongKhoang
                .Where(d => d.TrangThai == "Hoàn thành")
                .Sum(d => (double?)d.TongTien) ?? 0;

            // 5. Thống kê doanh thu theo từng ngày (Dùng class DoanhThuTheoNgayVM của bạn)
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

            // 6. Top 10 truyện bán chạy (Dùng class TopTruyenVM của bạn)
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
                    TenTruyen = g.Key.TenTruyen ?? "Không rõ",
                    TongSoLuong = g.Sum(x => x.SoLuong),
                    // Tính doanh thu từ chi tiết đơn hàng (SoLuong * DonGia)
                    DoanhThu = g.Sum(x => (double)x.DonGia * x.SoLuong)
                })
                .OrderByDescending(x => x.DoanhThu)
                .Take(10)
                .ToList();

            // 7. Khởi tạo ViewModel chính và gán dữ liệu
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