using MangaShop.Models;
using MangaShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class NvbTruyenController : BaseAdminController
    {
        private readonly MangaShopContext _context;

        public NvbTruyenController(MangaShopContext context)
        {
            _context = context;
        }

        // ================== INDEX ==================
        public IActionResult Index()
        {
            var truyen = _context.Truyens
                .Where(t => !t.IsDeleted)
                .Include(t => t.MaTheLoaiNavigation)
                .Include(t => t.TruyenTaps) // ✅ thêm để hiển thị số tập & tồn theo tập
                .ToList();

            return View(truyen);
        }


        // ================== CREATE ==================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.TheLoai = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("MaTruyen,TenTruyen,TacGia,Gia,MaTheLoai,MoTa,AnhBia,LoaiTruyen")] Truyen truyen)
        {
            if (ModelState.IsValid)
            {
                _context.Truyens.Add(truyen);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TheLoai = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", truyen.MaTheLoai);
            return View(truyen);
        }

        // ================== EDIT ==================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Thêm Include để nạp danh sách ảnh phụ từ Database lên View
            var truyen = _context.Truyens
                .Include(t => t.TruyenImages)
                .FirstOrDefault(t => t.MaTruyen == id);

            if (truyen == null) return NotFound();

            ViewBag.TheLoai = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", truyen.MaTheLoai);
            return View(truyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTruyen,TenTruyen,TacGia,Gia,MaTheLoai,MoTa,LoaiTruyen")] Truyen truyen, IFormFile coverImage, List<IFormFile> newImages)
        {
            if (id != truyen.MaTruyen) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTruyen = await _context.Truyens.FirstOrDefaultAsync(t => t.MaTruyen == id);
                    if (existingTruyen == null) return NotFound();

                    // Cập nhật thông tin cơ bản
                    existingTruyen.TenTruyen = truyen.TenTruyen;
                    existingTruyen.TacGia = truyen.TacGia;
                    existingTruyen.Gia = truyen.Gia;
                    existingTruyen.MaTheLoai = truyen.MaTheLoai;
                    existingTruyen.MoTa = truyen.MoTa;
                    existingTruyen.LoaiTruyen = truyen.LoaiTruyen;

                    // --- XỬ LÝ UPLOAD ẢNH BÌA MỚI ---
                    if (coverImage != null && coverImage.Length > 0)
                    {
                        // Tạo tên file duy nhất
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await coverImage.CopyToAsync(stream);
                        }

                        // Cập nhật tên file vào database
                        existingTruyen.AnhBia = fileName;
                    }

                    // --- XỬ LÝ NHIỀU ẢNH PHỤ (Giữ nguyên logic cũ của bạn) ---
                    if (newImages != null && newImages.Count > 0)
                    {
                        foreach (var file in newImages)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            _context.TruyenImages.Add(new TruyenImages
                            {
                                MaTruyen = truyen.MaTruyen,
                                Path = fileName,
                                DisplayOrder = 0
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Truyens.Any(e => e.MaTruyen == truyen.MaTruyen)) return NotFound();
                    else throw;
                }
            }
            ViewBag.TheLoai = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", truyen.MaTheLoai);
            return View(truyen);
        }

        // ================== DELETE ==================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var truyen = _context.Truyens
                .Include(t => t.MaTheLoaiNavigation)
                .FirstOrDefault(t => t.MaTruyen == id);

            if (truyen == null) return NotFound();
            return View(truyen);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var truyen = _context.Truyens.Find(id);
            if (truyen == null) return NotFound();

            // ✅ XÓA MỀM
            truyen.IsDeleted = true;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Trash()
        {
            var truyenDaAn = _context.Truyens
                .Where(t => t.IsDeleted)
                .Include(t => t.MaTheLoaiNavigation)
                .ToList();

            return View(truyenDaAn);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restore(int id)
        {
            var truyen = _context.Truyens.Find(id);
            if (truyen == null) return NotFound();

            truyen.IsDeleted = false;  // ✅ khôi phục
            _context.SaveChanges();

            return RedirectToAction(nameof(Trash)); // hoặc Index tùy bạn
        }
        // ================== NHAPKHO THEO TẬP ==================
        [HttpGet]
        public IActionResult NhapKhoTap()
        {
            var data = _context.Truyens
                .Where(t => !t.IsDeleted)
                .Include(t => t.TruyenTaps)
                .Select(t => new NhapKhoTapTruyenVM
                {
                    MaTruyen = t.MaTruyen,
                    TenTruyen = t.TenTruyen,
                    AnhBia = t.AnhBia,
                    Taps = t.TruyenTaps
                        .OrderBy(x => x.SoTap)
                        .Select(x => new NhapKhoTapItemVM
                        {
                            MaTap = x.MaTap,
                            SoTap = x.SoTap,
                            SoLuongTon = x.SoLuongTon,
                            SoLuongNhap = 0
                        }).ToList()
                })
                .ToList();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapKhoTap(List<NhapKhoTapTruyenVM> model)
        {
            if (model == null || !model.Any()) return RedirectToAction("NhapKhoTap");

            // 1. KIỂM TRA HỢP LỆ: Phải có ít nhất 1 tập có số lượng nhập > 0
            var coNhapHang = model.Any(tr => tr.Taps != null && tr.Taps.Any(tap => tap.SoLuongNhap > 0));

            if (!coNhapHang)
            {
                // Thêm thông báo lỗi để hiển thị ngoài View (nếu bạn có dùng ValidationSummary)
                ModelState.AddModelError("", "Vui lòng nhập số lượng cho ít nhất một tập truyện.");
                return View(model);
            }

            // 2. TẠO PHIẾU NHẬP (Chỉ tạo khi đã chắc chắn có hàng)
            var phieuNhap = new PhieuNhap
            {
                MaAdmin = 1, // ID Admin thực tế của bạn
                NgayNhap = DateTime.Now,
                TrangThai = "Đã nhập",
                TongTienNhap = 0 // Sẽ cập nhật sau khi tính toán
            };

            _context.PhieuNhaps.Add(phieuNhap);
            await _context.SaveChangesAsync(); // Lưu để lấy MaPhieuNhap tự sinh

            decimal tongTienPhieu = 0;

            // 3. XỬ LÝ CHI TIẾT VÀ CẬP NHẬT KHO
            foreach (var tr in model)
            {
                if (tr.Taps == null) continue;
                int tongNhapCuaTruyenNay = 0;

                foreach (var tapVm in tr.Taps)
                {
                    if (tapVm.SoLuongNhap <= 0) continue;

                    var tap = await _context.TruyenTaps.FirstOrDefaultAsync(x => x.MaTap == tapVm.MaTap);
                    if (tap != null)
                    {
                        // Tính giá nhập (70% giá bán lẻ)
                        decimal giaNhapKhuyenNghi = (decimal)(tap.Gia * 0.7);

                        // Lưu chi tiết phiếu nhập
                        var chiTiet = new ChiTietPhieuNhap
                        {
                            MaPhieuNhap = phieuNhap.MaPhieuNhap,
                            MaTruyen = tr.MaTruyen,
                            MaTap = tap.MaTap,
                            SoLuongNhap = tapVm.SoLuongNhap,
                            DonGiaNhap = giaNhapKhuyenNghi
                        };
                        _context.ChiTietPhieuNhaps.Add(chiTiet);

                        // Cập nhật tồn kho cho từng tập
                        tap.SoLuongTon += tapVm.SoLuongNhap;
                        tongNhapCuaTruyenNay += tapVm.SoLuongNhap;

                        // Cộng dồn vào tổng tiền phiếu
                        tongTienPhieu += (decimal)tapVm.SoLuongNhap * giaNhapKhuyenNghi;
                    }
                }

                // Cập nhật tổng tồn kho cho đầu truyện
                if (tongNhapCuaTruyenNay > 0)
                {
                    var truyen = await _context.Truyens.FindAsync(tr.MaTruyen);
                    if (truyen != null)
                    {
                        truyen.SoLuongTon += tongNhapCuaTruyenNay;
                    }
                }
            }

            // 4. CẬP NHẬT TỔNG TIỀN CUỐI CÙNG VÀ HOÀN TẤT
            phieuNhap.TongTienNhap = tongTienPhieu;
            await _context.SaveChangesAsync();

            return RedirectToAction("LichSuNhap");
        }
        public IActionResult LichSuNhap()
        {
            var data = _context.PhieuNhaps
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(ct => ct.MaTruyenNavigation)
                .Include(p => p.ChiTietPhieuNhaps)
                    .ThenInclude(ct => ct.MaTapNavigation)
                .OrderByDescending(p => p.NgayNhap)
                .ToList();

            return View(data);
        }
        // Thêm số lượng tập
        public IActionResult ThemTap(int id) // id là MaTruyen
        {
            var truyen = _context.Truyens.Include(t => t.TruyenTaps).FirstOrDefault(t => t.MaTruyen == id);
            if (truyen == null) return NotFound();

            // Tìm số tập lớn nhất hiện tại và cộng thêm 1
            int soTapHienTaiMax = truyen.TruyenTaps.Any() ? truyen.TruyenTaps.Max(x => x.SoTap) : 0;

            var model = new ThemTapVM
            {
                MaTruyen = id,
                TenTruyen = truyen.TenTruyen,
                SoTapTiepTheo = soTapHienTaiMax + 1,
                Gia = truyen.Gia // Lấy giá mặc định của truyện làm gợi ý
            };
            return View(model);
        }

        // 2. Xử lý lưu tập mới
        [HttpPost]
        [ValidateAntiForgeryToken] // Nên thêm cái này để bảo mật
        public IActionResult ThemTap(ThemTapVM model)
        {
            // Kiểm tra ModelState xem các trường bắt buộc như MaTruyen, SoTap đã có chưa
            if (ModelState.IsValid)
            {
                // 1. Thêm vào bảng TruyenTap
                var moi = new TruyenTap
                {
                    MaTruyen = model.MaTruyen,
                    SoTap = model.SoTapTiepTheo,
                    Gia = model.Gia,
                    // Nếu model.SoLuongThem null hoặc không nhập, mặc định là 0
                    SoLuongTon = model.SoLuongThem
                };
                _context.TruyenTaps.Add(moi);

                // 2. Cập nhật tổng số lượng tồn của bảng Truyen
                var truyen = _context.Truyens.Find(model.MaTruyen);
                if (truyen != null)
                {
                    // Cộng thêm số lượng mới vào tổng (nếu là 0 thì tổng không đổi)
                    // Điều này giúp truyện vẫn tồn tại nhưng tập mới nhất sẽ có tồn = 0 -> Hiện ở "Sắp phát hành"
                    truyen.SoLuongTon += model.SoLuongThem;
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu có lỗi (ví dụ chưa nhập Số tập), trả lại View cùng với thông báo
            return View(model);
        }

        // Action xóa ảnh phụ
        [HttpGet]
        public async Task<IActionResult> DeleteImage(int id)
        {
            // Tìm ảnh theo Id (đúng với Model bạn vừa gửi)
            var image = await _context.TruyenImages.FindAsync(id);

            if (image != null)
            {
                int maTruyen = image.MaTruyen; // Lưu lại mã truyện để quay về trang Edit

                // 1. Xóa file vật lý trong thư mục wwwroot/images
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", image.Path);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                // 2. Xóa dữ liệu trong database
                _context.TruyenImages.Remove(image);
                await _context.SaveChangesAsync();

                // Quay lại trang Edit của truyện vừa rồi
                return RedirectToAction("Edit", new { id = maTruyen });
            }

            return RedirectToAction("Index");
        }
    }
}
