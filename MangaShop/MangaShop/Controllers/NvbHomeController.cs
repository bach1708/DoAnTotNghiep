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

        public NvbHomeController(MangaShopContext context)
        {
            _context = context;
        }

        // ================== TRANG CHỦ ==================
        public IActionResult Home()
        {
            // 1. Lọc danh sách gốc (Chỉ lấy truyện chưa xóa + Include TruyenTaps để tính tồn kho)
            var tatCaTruyen = _context.Truyens
                .Include(t => t.TruyenTaps)
                .Where(t => !t.IsDeleted)
                .ToList();

            // 2. Danh sách 1: Sản phẩm khuyến mãi (Có ít nhất 1 tập còn hàng + Giá rẻ)
            var promo = tatCaTruyen
                .Where(t => t.TruyenTaps.Any(tap => tap.SoLuongTon > 0) && t.Gia < 50000)
                .OrderByDescending(t => t.MaTruyen)
                .Take(10)
                .ToList();

            var promoIds = promo.Select(p => p.MaTruyen).ToList();

            // 3. Danh sách 2: Sắp phát hành
            // Logic: Truyện có ít nhất một tập mà số lượng tồn bằng 0 (Tập sắp phát hành)
            // Hoặc truyện chưa có tập nào.
            var upcoming = tatCaTruyen
                .Where(t => t.TruyenTaps.Any() &&
                            t.TruyenTaps.OrderByDescending(tap => tap.SoTap)
                                       .FirstOrDefault().SoLuongTon == 0)
                .OrderByDescending(t => t.MaTruyen)
                .Take(10)
                .ToList();

            // 4. Danh sách 3: Mới nhất trong tuần (Giữ nguyên như cũ)
            var newest = tatCaTruyen
                .Where(t => t.TruyenTaps.Any(tap => tap.SoLuongTon > 0) && !promoIds.Contains(t.MaTruyen))
                .OrderByDescending(t => t.MaTruyen)
                .Take(10)
                .ToList();

            // Gửi qua View
            ViewBag.Upcoming = upcoming;
            ViewBag.Promo = promo;

            return View(newest);
        }

        // ================== TÌM KIẾM ==================
        public IActionResult TimKiem(string? keyword)
        {
            keyword = (keyword ?? "").Trim();
            if (string.IsNullOrEmpty(keyword))
                return RedirectToAction("DanhMuc");

            // Load kèm TruyenTaps để View có thể hiển thị số lượng nếu cần
            var query = _context.Truyens
                .Include(t => t.MaTheLoaiNavigation)
                .Include(t => t.TruyenTaps)
                .Where(t => !t.IsDeleted);

            var keyLow = keyword.ToLower();
            var keyNorm = TextHelper.NormalizeForSearch(keyword);

            if (keyLow.Contains("truyện tranh") || keyLow == "manga" || keyNorm == "truyen tranh")
            {
                query = query.Where(t => t.LoaiTruyen == 0);
            }
            else if (keyLow.Contains("truyện chữ") || keyLow == "light novel" || keyLow == "novel" || keyNorm == "truyen chu")
            {
                query = query.Where(t => t.LoaiTruyen == 1);
            }
            else
            {
                query = query.Where(t =>
                    t.TenTruyen.Contains(keyword) ||
                    (t.TacGia ?? "").Contains(keyword) ||
                    (t.MaTheLoaiNavigation != null && t.MaTheLoaiNavigation.TenTheLoai.Contains(keyword))
                );
            }

            var result = query.OrderBy(t => t.TenTruyen).ToList();

            if (!result.Any() && !string.IsNullOrEmpty(keyNorm))
            {
                result = _context.Truyens
                    .Include(t => t.MaTheLoaiNavigation)
                    .Include(t => t.TruyenTaps)
                    .Where(t => !t.IsDeleted)
                    .AsEnumerable()
                    .Where(t =>
                        TextHelper.NormalizeForSearch(t.TenTruyen).Contains(keyNorm) ||
                        TextHelper.NormalizeForSearch(t.TacGia ?? "").Contains(keyNorm)
                    )
                    .ToList();
            }

            ViewBag.Keyword = keyword;
            ViewBag.Count = result.Count;

            return View(result);
        }

        // ================== DANH MỤC ==================
        public IActionResult DanhMuc(int? maTheLoai, int? format, int page = 1)
        {
            int pageSize = 9;
            var theLoai = _context.TheLoais.ToList();

            // Load kèm TruyenTaps để xử lý logic hiển thị
            var query = _context.Truyens
                .Include(t => t.TruyenTaps)
                .Where(t => !t.IsDeleted).AsQueryable();

            if (maTheLoai.HasValue)
            {
                query = query.Where(t => t.MaTheLoai == maTheLoai);
            }

            if (format.HasValue)
            {
                query = query.Where(t => t.DinhDang == format);
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (page < 1) page = 1;

            var result = query
                .OrderByDescending(t => t.MaTruyen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TheLoai = theLoai;
            ViewBag.MaTheLoai = maTheLoai;
            ViewBag.Format = format;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(result);
        }

        // ================== CHI TIẾT TRUYỆN ==================
        public IActionResult ChiTiet(int id, string? returnUrl = null)
        {
            var truyen = _context.Truyens
                .Include(t => t.MaTheLoaiNavigation)
                .Include(t => t.TruyenTaps)
                .Include(t => t.TruyenImages)
                .Include(t => t.DanhGias)
                    .ThenInclude(d => d.MaKhachHangNavigation)
                .Include(t => t.DanhGias)
                    .ThenInclude(d => d.MaTapNavigation)
                .FirstOrDefault(t => t.MaTruyen == id);

            if (truyen == null) return NotFound();

            ViewBag.ReturnUrl = returnUrl;
            return View(truyen);
        }

        public IActionResult LichPhatHanh()
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, 1, 1);
            var endDate = new DateTime(now.Year, now.Month, 1).AddMonths(1);

            var data = _context.LichPhatHanhs
                .Include(x => x.MaTruyenNavigation)
                .Where(x => x.TrangThai == true && x.NgayPhatHanh >= startDate && x.NgayPhatHanh < endDate)
                .OrderByDescending(x => x.NgayTao) // Tin mới đăng nhất lên đầu
                .Select(x => new LichItem
                {
                    NgayDang = x.NgayTao.ToString("dd/MM/yyyy"), // Cột đầu tiên
                    NgayPhatHanh = x.NgayPhatHanh.ToString("dd/MM/yyyy"), // Cột mới thêm
                    TenTruyen = x.MaTruyenNavigation.TenTruyen ?? "Không rõ",
                    Tap = x.MaTap.HasValue ? x.MaTap.Value.ToString() : "",
                    Gia = x.GiaDuKien.HasValue ? x.GiaDuKien.Value.ToString("N0") + " đ" : "Chưa rõ",
                    GhiChu = x.GhiChu ?? "",
                    LinkChiTiet = "/NvbHome/ChiTiet/" + x.MaTruyen
                }).ToList();

            ViewBag.ThangHienTai = $"{now.Month:D2}/{now.Year}";
            return View(data);
        }

        // ================== THÀNH VIÊN ==================
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

            var donQuery = _context.DonHangs
                .Where(d => d.MaKhachHang == userId.Value && d.TrangThai == "Hoàn thành");

            var donHangs = donQuery
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            double tongChiTieu = (double)donHangs.Sum(x => x.TongTien);
            int soDon = donHangs.Count;

            string hang = MemberTierHelper.GetTier(tongChiTieu);
            HttpContext.Session.SetString("UserRank", hang);

            string hangTiepTheo;
            double mucTieu;

            if (tongChiTieu >= 5000000)
            {
                hangTiepTheo = "Kim cương";
                mucTieu = 5000000;
            }
            else if (tongChiTieu >= 2000000)
            {
                hangTiepTheo = "Kim cương";
                mucTieu = 5000000;
            }
            else if (tongChiTieu >= 500000)
            {
                hangTiepTheo = "Vàng";
                mucTieu = 2000000;
            }
            else
            {
                hangTiepTheo = "Bạc";
                mucTieu = 500000;
            }

            var phanTram = mucTieu <= 0 ? 0 : (int)Math.Round((tongChiTieu / mucTieu) * 100.0);
            if (phanTram > 100) phanTram = 100;

            var vm = new ThanhVienCuaToiVM
            {
                HoTen = kh.HoTen ?? "",
                Email = kh.Email ?? "",
                AnhDaiDien = kh.AnhDaiDien,
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
                    TongTien = (double)d.TongTien,
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