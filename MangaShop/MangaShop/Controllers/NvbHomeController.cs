using MangaShop.Helpers;
using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using MangaShop.Models.ViewModels;
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
            // Lấy truyện mới nhất (lấy theo MaTruyen mới nhất)
            // Bạn có thể đổi Take(8) / Take(12) tùy muốn hiển thị bao nhiêu
            var newest = _context.Truyens
                .OrderByDescending(t => t.MaTruyen)
                .Take(10)
                .ToList();

            return View(newest);
        }
        public IActionResult TimKiem(string? keyword)
        {
            keyword = (keyword ?? "").Trim();
            if (string.IsNullOrEmpty(keyword))
                return RedirectToAction("DanhMuc");

            // từ khoá chuẩn hoá (bỏ dấu + bỏ khoảng trắng)
            var keyNorm = TextHelper.NormalizeForSearch(keyword);

            // lọc sơ bộ SQL (nhanh)
            var candidates = _context.Truyens
                .Include(t => t.MaTheLoaiNavigation)
                .Where(t =>
                    t.TenTruyen.Contains(keyword) ||
                    (t.TacGia ?? "").Contains(keyword) ||
                    (t.MaTheLoaiNavigation != null && t.MaTheLoaiNavigation.TenTheLoai.Contains(keyword))
                )
                .ToList();

            // nếu lọc sơ bộ không ra, lấy thêm một ít để lọc bằng normalize
            // (tránh trường hợp "dragonball" không match "Dragon Ball")
            if (!candidates.Any())
            {
                candidates = _context.Truyens
                    .Include(t => t.MaTheLoaiNavigation)
                    .ToList();
            }

            // lọc bằng normalize (linh hoạt)
            var result = candidates.Where(t =>
            {
                var tenNorm = TextHelper.NormalizeForSearch(t.TenTruyen);
                var tacGiaNorm = TextHelper.NormalizeForSearch(t.TacGia);
                var theLoaiNorm = TextHelper.NormalizeForSearch(t.MaTheLoaiNavigation?.TenTheLoai);

                // match theo chuỗi liền nhau: dragonball
                if (!string.IsNullOrEmpty(keyNorm) &&
                    (tenNorm.Contains(keyNorm) || tacGiaNorm.Contains(keyNorm) || theLoaiNorm.Contains(keyNorm)))
                    return true;

                // match theo từng từ: "dragon ball"
                var tokens = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return tokens.All(tok =>
                {
                    var tokNorm = TextHelper.NormalizeForSearch(tok);
                    return tenNorm.Contains(tokNorm) || tacGiaNorm.Contains(tokNorm) || theLoaiNorm.Contains(tokNorm);
                });
            })
            .Distinct()
            .OrderBy(t => t.TenTruyen)
            .ToList();

            ViewBag.Keyword = keyword;
            ViewBag.Count = result.Count;

            return View(result);
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
        // ================== CHI TIẾT TRUYỆN ==================
        public IActionResult ChiTiet(int id, string? returnUrl = null)
        {
            var truyen = _context.Truyens
                .Include(t => t.MaTheLoaiNavigation)
                .Include(t => t.TruyenTaps) // ✅ bắt buộc để view đọc tập
                .Include(t => t.DanhGias)
                    .ThenInclude(d => d.MaKhachHangNavigation)
                .FirstOrDefault(t => t.MaTruyen == id);

            if (truyen == null) return NotFound();

            ViewBag.ReturnUrl = returnUrl;
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
        public IActionResult ThanhVienCuaToi()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "NvbAccount");

            var kh = _context.KhachHangs.FirstOrDefault(x => x.MaKhachHang == userId.Value);
            if (kh == null) return RedirectToAction("Login", "NvbAccount");

            // ✅ chỉ tính đơn không bị hủy (bạn có thể đổi điều kiện theo dự án)
            var donQuery = _context.DonHangs
                .Where(d => d.MaKhachHang == userId.Value)
                .Where(d => d.TrangThai != "Huỷ" && d.TrangThai != "Đã Hủy");

            var donHangs = donQuery
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            var tongChiTieu = donHangs.Sum(x => x.TongTien);
            var soDon = donHangs.Count;

            string hang, hangTiepTheo;
            double mucTieu;

            if (tongChiTieu >= 10_000_000)
            {
                hang = "Vàng";
                hangTiepTheo = "Vàng";
                mucTieu = 10_000_000;
            }
            else if (tongChiTieu >= 5_000_000)
            {
                hang = "Bạc";
                hangTiepTheo = "Vàng";
                mucTieu = 10_000_000;
            }
            else if (tongChiTieu >= 1_000_000)
            {
                hang = "Đồng";
                hangTiepTheo = "Bạc";
                mucTieu = 5_000_000;
            }
            else
            {
                hang = "Chưa có";
                hangTiepTheo = "Đồng";
                mucTieu = 1_000_000;
            }

            var phanTram = mucTieu <= 0 ? 0 : (int)Math.Round((tongChiTieu / mucTieu) * 100.0);
            if (phanTram > 100) phanTram = 100;
            if (phanTram < 0) phanTram = 0;

            var vm = new ThanhVienCuaToiVM
            {
                HoTen = kh.HoTen ?? "",
                Email = kh.Email ?? "",
                SoDienThoai = kh.SoDienThoai,
                DiaChi = kh.DiaChi,

                TongChiTieu = tongChiTieu,
                SoDonHang = soDon,

                HangThanhVien = hang,
                HangTiepTheo = hangTiepTheo,
                MucTieuTiepTheo = mucTieu,
                PhanTramTienDo = phanTram,

                DonHangGanDay = donHangs.Take(10).Select(d => new DonHangLiteVM
                {
                    MaDonHang = d.MaDonHang,
                    NgayDat = d.NgayDat,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai
                }).ToList()
            };

            return View(vm);
        }

        // ================== TIN TỨC ==================
        public IActionResult TinTuc(string? loai)
        {
            var query = _context.BaiViets
                .Where(b => b.TrangThai == true)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(loai))
                query = query.Where(b => b.Loai == loai);

            var data = query
                .OrderByDescending(b => b.NgayDang)
                .ToList();

            ViewBag.Loai = loai;
            return View(data);
        }

        // ================== CHI TIẾT BÀI VIẾT ==================
        public IActionResult TinTucChiTiet(int id)
        {
            var bai = _context.BaiViets.FirstOrDefault(b => b.MaBaiViet == id && b.TrangThai == true);
            if (bai == null) return NotFound();

            return View(bai);
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
