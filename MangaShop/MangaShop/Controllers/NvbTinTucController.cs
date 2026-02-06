using MangaShop.Models;
using MangaShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;

namespace MangaShop.Controllers
{
    public class NvbTinTucController : BaseAdminController
    {
        private readonly MangaShopContext _context;
        private readonly IWebHostEnvironment _env;

        public NvbTinTucController(MangaShopContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ===== DANH SÁCH BÀI VIẾT =====
        public IActionResult Index(string? keyword, string? loai)
        {
            var q = _context.BaiViets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                q = q.Where(x => x.TieuDe.Contains(keyword) || (x.TomTat ?? "").Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(loai))
            {
                q = q.Where(x => x.Loai == loai);
            }

            ViewBag.Keyword = keyword;
            ViewBag.Loai = loai;

            var data = q.OrderByDescending(x => x.NgayDang).ToList();
            return View(data);
        }

        // ===== THÊM MỚI =====
        [HttpGet]
        public IActionResult Create() => View(new BaiVietFormVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BaiVietFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            string? fileName = SaveImage(vm.AnhUpload);

            var entity = new BaiViet
            {
                TieuDe = vm.TieuDe,
                TomTat = vm.TomTat,
                NoiDung = vm.NoiDung,
                Loai = vm.Loai,
                TrangThai = vm.TrangThai,
                AnhDaiDien = fileName, // Lưu tên file thuần túy
                NgayDang = DateTime.Now
            };

            _context.BaiViets.Add(entity);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ===== CẬP NHẬT =====
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var bai = _context.BaiViets.FirstOrDefault(x => x.MaBaiViet == id);
            if (bai == null) return NotFound();

            var vm = new BaiVietFormVM
            {
                MaBaiViet = bai.MaBaiViet,
                TieuDe = bai.TieuDe,
                TomTat = bai.TomTat,
                NoiDung = bai.NoiDung,
                Loai = bai.Loai,
                TrangThai = bai.TrangThai,
                AnhDaiDienHienTai = bai.AnhDaiDien
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BaiVietFormVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var bai = _context.BaiViets.FirstOrDefault(x => x.MaBaiViet == vm.MaBaiViet);
            if (bai == null) return NotFound();

            bai.TieuDe = vm.TieuDe;
            bai.TomTat = vm.TomTat;
            bai.NoiDung = vm.NoiDung;
            bai.Loai = vm.Loai;
            bai.TrangThai = vm.TrangThai;

            // Xử lý upload ảnh mới
            if (vm.AnhUpload != null && vm.AnhUpload.Length > 0)
            {
                // 1. Lưu ảnh mới
                var newFile = SaveImage(vm.AnhUpload);

                // 2. Xóa ảnh cũ trên server (nếu có)
                if (!string.IsNullOrEmpty(bai.AnhDaiDien))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, "images", "baiviet", bai.AnhDaiDien);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                bai.AnhDaiDien = newFile;
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ===== CHI TIẾT & XÓA =====
        public IActionResult Details(int id)
        {
            var bai = _context.BaiViets.FirstOrDefault(x => x.MaBaiViet == id);
            if (bai == null) return NotFound();
            return View(bai);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var bai = _context.BaiViets.FirstOrDefault(x => x.MaBaiViet == id);
            if (bai == null) return NotFound();
            return View(bai);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var bai = _context.BaiViets.FirstOrDefault(x => x.MaBaiViet == id);
            if (bai != null)
            {
                // Xóa ảnh trước khi xóa bản ghi
                if (!string.IsNullOrEmpty(bai.AnhDaiDien))
                {
                    var path = Path.Combine(_env.WebRootPath, "images", "baiviet", bai.AnhDaiDien);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }
                _context.BaiViets.Remove(bai);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // ===== HELPER: LƯU ẢNH =====
        private string? SaveImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var folder = Path.Combine(_env.WebRootPath, "images", "baiviet");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return fileName; // Trả về mỗi tên file để DB gọn nhẹ
        }
    }
}