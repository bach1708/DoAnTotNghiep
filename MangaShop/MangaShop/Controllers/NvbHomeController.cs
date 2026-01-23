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
            // 1. Lọc danh sách gốc (Chỉ lấy truyện chưa xóa)
            var tatCaTruyen = _context.Truyens.Where(t => !t.IsDeleted);

            // 2. Danh sách 1: Sản phẩm khuyến mãi (Có hàng + Giá rẻ)
            // Ưu tiên lấy những truyện này ra trước
            var promo = tatCaTruyen
                .Where(t => t.SoLuongTon > 0 && t.Gia < 50000)
                .OrderByDescending(t => t.MaTruyen)
                .Take(10)
                .ToList();

            // Lấy danh sách ID của promo để loại trừ ở phần Newest
            var promoIds = promo.Select(p => p.MaTruyen).ToList();

            // 3. Danh sách 2: Sắp phát hành (Không còn hàng/Chưa nhập hàng)
            var upcoming = tatCaTruyen
                .Where(t => t.SoLuongTon == 0)
                .OrderByDescending(t => t.MaTruyen)
                .Take(10)
                .ToList();

            // 4. Danh sách 3: Mới nhất trong tuần (Có hàng + KHÔNG nằm trong promo)
            var newest = tatCaTruyen
                .Where(t => t.SoLuongTon > 0 && !promoIds.Contains(t.MaTruyen))
                .OrderByDescending(t => t.MaTruyen)
                .Take(10)
                .ToList();

            // Gửi qua View
            ViewBag.Upcoming = upcoming;
            ViewBag.Promo = promo;

            return View(newest);
        }
        public IActionResult TimKiem(string? keyword)
        {
            keyword = (keyword ?? "").Trim();
            if (string.IsNullOrEmpty(keyword))
                return RedirectToAction("DanhMuc");

            // 1. Lấy danh sách truyện CHƯA XÓA MỀM (IsDeleted == false)
            var query = _context.Truyens
                .Include(t => t.MaTheLoaiNavigation)
                .Where(t => !t.IsDeleted); // Lọc bỏ truyện đã ẩn ở đây

            // 2. Chuẩn hóa từ khóa để so sánh (ví dụ: "Truyện Tranh" -> "truyen tranh")
            var keyLow = keyword.ToLower();
            var keyNorm = TextHelper.NormalizeForSearch(keyword);

            // 3. Logic nhận diện Loại Truyện qua Keyword
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
                // 4. Nếu không phải từ khóa loại truyện, tìm theo tên/tác giả/thể loại
                query = query.Where(t =>
                    t.TenTruyen.Contains(keyword) ||
                    (t.TacGia ?? "").Contains(keyword) ||
                    (t.MaTheLoaiNavigation != null && t.MaTheLoaiNavigation.TenTheLoai.Contains(keyword))
                );
            }

            var result = query.OrderBy(t => t.TenTruyen).ToList();

            // 5. Nếu tìm kiếm thông thường không ra, dùng thêm Normalize để tìm (Dragonball vs Dragon Ball)
            if (!result.Any() && !string.IsNullOrEmpty(keyNorm))
            {
                result = _context.Truyens
                    .Include(t => t.MaTheLoaiNavigation)
                    .Where(t => !t.IsDeleted)
                    .AsEnumerable() // Chuyển về bộ nhớ để dùng hàm TextHelper
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
            int pageSize = 9; // Hiển thị 9 truyện mỗi trang
            var theLoai = _context.TheLoais.ToList();

            // 1. Khởi tạo query gốc
            var query = _context.Truyens.Where(t => !t.IsDeleted).AsQueryable();

            // 2. Lọc theo Thể loại
            if (maTheLoai.HasValue)
            {
                query = query.Where(t => t.MaTheLoai == maTheLoai);
            }

            // 3. Lọc theo Hình thức (DinhDang)
            if (format.HasValue)
            {
                query = query.Where(t => t.DinhDang == format);
            }

            // 4. Tính toán phân trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Đảm bảo số trang luôn hợp lệ
            if (page < 1) page = 1;

            // 5. Lấy dữ liệu trang hiện tại
            var result = query
                .OrderByDescending(t => t.MaTruyen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 6. Gửi dữ liệu qua ViewBag
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
                .Include(t => t.TruyenTaps) // ✅ bắt buộc để view đọc tập
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
            var lichThangNay = new List<LichItem>
            {
                new LichItem { Ngay = "04/08", TenTruyen = "KINGDOM", Tap = "54", Gia = "45.000", GhiChu = "Tái bản", LinkChiTiet = "/NvbHome/ChiTiet/1" },
                new LichItem { Ngay = "04/08", TenTruyen = "FIRE FORCE", Tap = "10", Gia = "43.000", GhiChu = "", LinkChiTiet = "/NvbHome/ChiTiet/2" },
                new LichItem { Ngay = "18/08", TenTruyen = "THANH TRA AKECHI", Tap = "1", Gia = "50.000", GhiChu = "TRUYỆN MỚI", LinkChiTiet = "" },
                new LichItem { Ngay = "25/08", TenTruyen = "NỤ HÔN TINH NGHỊCH", Tap = "1", Gia = "Chưa rõ", GhiChu = "Cực hot", LinkChiTiet = "" }
            };

            ViewBag.Thang = "08/2023"; // Gửi tiêu đề tháng ra View
            return View(lichThangNay);
        }

        public IActionResult ThanhVien()
        {

            return View();
        }
        // ================== TRANG THÀNH VIÊN CỦA TÔI ==================
        public IActionResult ThanhVienCuaToi()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "NvbAccount");

            var kh = _context.KhachHangs.FirstOrDefault(x => x.MaKhachHang == userId.Value);
            if (kh == null) return RedirectToAction("Login", "NvbAccount");

            // 1. Chỉ tính các đơn đã "Hoàn thành" để đồng bộ với Admin
            var donQuery = _context.DonHangs
                .Where(d => d.MaKhachHang == userId.Value && d.TrangThai == "Hoàn thành");

            var donHangs = donQuery
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            // 2. Ép kiểu decimal từ DB sang double để gán vào ViewModel (Khớp với model bạn gửi)
            double tongChiTieu = (double)donHangs.Sum(x => x.TongTien);
            int soDon = donHangs.Count;

            // 3. Dùng Helper để lấy tên hạng (Đồng bộ tuyệt đối với Admin)
            string hang = MemberTierHelper.GetTier(tongChiTieu);

            // 4. Cập nhật Session để Navbar thay đổi ngay lập tức
            HttpContext.Session.SetString("UserRank", hang);

            string hangTiepTheo;
            double mucTieu;

            // 5. Xác định mục tiêu thăng hạng (Khớp với logic hiển thị)
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

            // 6. Tính % tiến độ (Làm tròn số nguyên cho ProgressBar)
            var phanTram = mucTieu <= 0 ? 0 : (int)Math.Round((tongChiTieu / mucTieu) * 100.0);
            if (phanTram > 100) phanTram = 100;

            // 7. Gán dữ liệu vào ViewModel
            var vm = new ThanhVienCuaToiVM
            {
                HoTen = kh.HoTen ?? "",
                Email = kh.Email ?? "",
                SoDienThoai = kh.SoDienThoai,
                DiaChi = kh.DiaChi,

                TongChiTieu = tongChiTieu, // double
                SoDonHang = soDon,

                HangThanhVien = hang,
                HangTiepTheo = hangTiepTheo,
                MucTieuTiepTheo = mucTieu,
                PhanTramTienDo = phanTram,

                DonHangGanDay = donHangs.Take(10).Select(d => new DonHangLiteVM
                {
                    MaDonHang = d.MaDonHang,
                    NgayDat = d.NgayDat,
                    TongTien = (double)d.TongTien, // Ép kiểu sang double ở đây
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
