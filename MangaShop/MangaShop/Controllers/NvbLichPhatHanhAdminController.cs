using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    // ✅ KẾ THỪA BaseAdminController
    public class NvbLichPhatHanhAdminController : BaseAdminController
    {
        private readonly MangaShopContext _context;

        // ✅ Inject DbContext
        public NvbLichPhatHanhAdminController(MangaShopContext context)
        {
            _context = context;
        }

        // ================== DANH SÁCH LỊCH PHÁT HÀNH ==================
        public IActionResult Index()
        {
            var data = _context.LichPhatHanhs
                .Include(x => x.MaTruyenNavigation)
                .Include(x => x.MaTapNavigation)
                .OrderByDescending(x => x.NgayPhatHanh)
                .ToList();

            return View(data);
        }

        // ================== TẠO LỊCH PHÁT HÀNH ==================
        [HttpGet]
        public IActionResult Create()
        {
            // 1. Lấy danh sách truyện để nạp vào dropdown chọn truyện
            var dsTruyen = _context.Truyens
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.TenTruyen)
                .ToList();

            // 2. Chuẩn bị dữ liệu JSON cho JavaScript (Xử lý khi người dùng chọn truyện)
            // Quan trọng: Lấy SoTap lớn nhất từ bảng TruyenTaps để gợi ý chính xác tập tiếp theo
            ViewBag.TruyenData = _context.Truyens
                .Where(t => !t.IsDeleted)
                .Select(t => new {
                    maTruyen = t.MaTruyen,
                    tacGia = t.TacGia ?? "Chưa rõ",
                    anhBia = t.AnhBia ?? "noimage.jpg",
                    // Lấy số tập cao nhất hiện có trong kho
                    tapHienTaiMax = t.TruyenTaps.Any() ? t.TruyenTaps.Max(tap => tap.SoTap) : 0,
                    giaNiemYet = t.Gia
                }).ToList();

            // Nạp vào SelectList để hiển thị trong <select>
            ViewBag.Truyen = new SelectList(dsTruyen, "MaTruyen", "TenTruyen");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LichPhatHanh model)
        {
            // Loại bỏ kiểm tra các navigation object để tránh lỗi ModelState không hợp lệ
            ModelState.Remove("MaTruyenNavigation");
            ModelState.Remove("MaTapNavigation");

            if (ModelState.IsValid)
            {
                model.NgayTao = DateTime.Now;
                model.TrangThai = true; // Mặc định hiển thị khi mới tạo

                _context.LichPhatHanhs.Add(model);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi validation, nạp lại dữ liệu để hiển thị lại form
            var dsTruyen = _context.Truyens.Where(t => !t.IsDeleted).ToList();
            ViewBag.Truyen = new SelectList(dsTruyen, "MaTruyen", "TenTruyen", model.MaTruyen);

            // Nạp lại dữ liệu JSON cho JS
            ViewBag.TruyenData = dsTruyen.Select(t => new {
                maTruyen = t.MaTruyen,
                tacGia = t.TacGia ?? "Chưa rõ",
                anhBia = t.AnhBia ?? "noimage.jpg",
                tapHienTaiMax = t.TruyenTaps.Any() ? t.TruyenTaps.Max(tap => tap.SoTap) : 0,
                giaNiemYet = t.Gia
            }).ToList();

            return View(model);
        }
        // API lấy danh sách tập khi chọn Truyện
        public JsonResult GetTapsByTruyen(int maTruyen)
        {
            var taps = _context.TruyenTaps
                .Where(t => t.MaTruyen == maTruyen)
                .Select(t => new { id = t.MaTap, name = "Tập " + t.SoTap })
                .ToList();
            return Json(taps);
        }
        // ================== SỬA LỊCH PHÁT HÀNH ==================
        public IActionResult Edit(int id)
        {
            var item = _context.LichPhatHanhs.FirstOrDefault(x => x.MaLich == id);
            if (item == null) return NotFound();

            // Thêm đoạn này để JS có dữ liệu hiển thị ảnh/giá/tác giả
            ViewBag.TruyenData = _context.Truyens.Select(t => new {
                maTruyen = t.MaTruyen,
                tacGia = t.TacGia,
                anhBia = t.AnhBia,
                tapHienTaiMax = t.TruyenTaps.Any() ? t.TruyenTaps.Max(tap => tap.SoTap) : 0,
                giaNiemYet = t.Gia
            }).ToList();

            ViewBag.Truyen = new SelectList(_context.Truyens.Where(t => !t.IsDeleted), "MaTruyen", "TenTruyen", item.MaTruyen);

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(LichPhatHanh model)
        {
            var item = _context.LichPhatHanhs.FirstOrDefault(x => x.MaLich == model.MaLich);
            if (item == null) return NotFound();

            item.MaTruyen = model.MaTruyen;
            item.MaTap = model.MaTap;
            item.NgayPhatHanh = model.NgayPhatHanh;
            item.GiaDuKien = model.GiaDuKien;
            item.GhiChu = model.GhiChu;
            item.TrangThai = model.TrangThai;
            item.NgayCapNhat = DateTime.Now;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ================== XOÁ LỊCH PHÁT HÀNH ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var item = _context.LichPhatHanhs.Find(id);
            if (item != null)
            {
                _context.LichPhatHanhs.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
