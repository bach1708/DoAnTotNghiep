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
            var truyen = _context.Truyens.Find(id);
            if (truyen == null) return NotFound();

            ViewBag.TheLoai = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", truyen.MaTheLoai);
            return View(truyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("MaTruyen,TenTruyen,TacGia,Gia,MaTheLoai,MoTa,AnhBia,LoaiTruyen")] Truyen truyen)
        {
            if (ModelState.IsValid)
            {
                _context.Truyens.Update(truyen);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
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


        // ================== NHAPKHO ==================
        public IActionResult NhapKho()
        {
            var data = _context.Truyens.Select(t => new NhapKhoVM
            {
                MaTruyen = t.MaTruyen,
                TenTruyen = t.TenTruyen,
                AnhBia = t.AnhBia,
                SoLuongTon = t.SoLuongTon,
                SoLuongNhap = 0
            }).ToList();

            return View(data);
        }

        [HttpPost]
        public IActionResult NhapKho(List<NhapKhoVM> model)
        {
            foreach (var item in model)
            {
                if (item.SoLuongNhap > 0)
                {
                    var truyen = _context.Truyens
                        .FirstOrDefault(t => t.MaTruyen == item.MaTruyen);

                    if (truyen != null)
                    {
                        truyen.SoLuongTon += item.SoLuongNhap;
                    }
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        // ================== NHAPKHO THEO TẬP ==================
        [HttpGet]
        public IActionResult NhapKhoTap()
        {
            var data = _context.Truyens
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
        public IActionResult NhapKhoTap(List<NhapKhoTapTruyenVM> model)
        {
            if (model == null || model.Count == 0)
                return RedirectToAction("NhapKhoTap");

            foreach (var tr in model)
            {
                // ✅ Truyện không có tập => bỏ qua, không lỗi
                if (tr.Taps == null || tr.Taps.Count == 0)
                    continue;

                foreach (var tapVm in tr.Taps)
                {
                    if (tapVm.SoLuongNhap <= 0)
                        continue;

                    var tap = _context.TruyenTaps.FirstOrDefault(x => x.MaTap == tapVm.MaTap);
                    if (tap != null)
                    {
                        tap.SoLuongTon += tapVm.SoLuongNhap;
                    }
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
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
        public IActionResult ThemTap(ThemTapVM model)
        {
            if (ModelState.IsValid)
            {
                // 1. Thêm vào bảng TruyenTap
                var moi = new TruyenTap
                {
                    MaTruyen = model.MaTruyen,
                    SoTap = model.SoTapTiepTheo,
                    Gia = model.Gia,
                    SoLuongTon = model.SoLuongThem
                };
                _context.TruyenTaps.Add(moi);

                // 2. Cập nhật tổng số lượng tồn của bảng Truyen
                var truyen = _context.Truyens.Find(model.MaTruyen);
                if (truyen != null)
                {
                    truyen.SoLuongTon += model.SoLuongThem;
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }


    }
}
