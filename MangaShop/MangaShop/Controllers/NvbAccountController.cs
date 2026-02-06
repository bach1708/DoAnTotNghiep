using Microsoft.AspNetCore.Mvc;
using MangaShop.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using MangaShop.Helpers;

namespace MangaShop.Controllers
{
    public class NvbAccountController : Controller
    {
        private readonly MangaShopContext _context;
        private readonly IWebHostEnvironment _env;

        // Gộp chung vào 1 Constructor duy nhất để tránh lỗi
        public NvbAccountController(MangaShopContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult Login() => View("NvbUserLogin");

        [HttpPost]
        public IActionResult Login(string Email, string MatKhau)
        {
            var user = _context.KhachHangs
        .FirstOrDefault(u => u.Email == Email && u.MatKhau == MatKhau);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.MaKhachHang);
                HttpContext.Session.SetString("UserName", user.HoTen ?? "");

                // --- THÊM DÒNG NÀY ĐỂ LƯU ẢNH VÀO SESSION ---
                HttpContext.Session.SetString("UserAvatar", user.AnhDaiDien ?? "default-avatar.png");

                SyncCartToDb(user.MaKhachHang);
                return RedirectToAction("Home", "NvbHome");
        }
            ViewBag.Error = "Email hoặc mật khẩu không đúng";
            return View("NvbUserLogin");
        }

        private void SyncCartToDb(int userId)
        {
            var sessionCart = HttpContext.Session.GetObject<List<CartItem>>("CART");
            if (sessionCart == null || !sessionCart.Any()) return;

            var dbGioHang = _context.GioHangs.FirstOrDefault(g => g.MaKhachHang == userId);
            if (dbGioHang == null)
            {
                dbGioHang = new GioHang { MaKhachHang = userId, NgayTao = DateTime.Now };
                _context.GioHangs.Add(dbGioHang);
                _context.SaveChanges();
            }

            foreach (var item in sessionCart)
            {
                var existingItem = _context.ChiTietGioHangs
                    .FirstOrDefault(ct => ct.MaGioHang == dbGioHang.MaGioHang && ct.MaTap == item.MaTap);

                if (existingItem != null) existingItem.SoLuong += item.SoLuong;
                else
                {
                    _context.ChiTietGioHangs.Add(new ChiTietGioHang
                    {
                        MaGioHang = dbGioHang.MaGioHang,
                        MaTruyen = item.MaTruyen,
                        MaTap = item.MaTap,
                        SoLuong = item.SoLuong
                    });
                }
            }
            _context.SaveChanges();
            HttpContext.Session.Remove("CART");
        }

        [HttpGet]
        public IActionResult Register() => View("NvbUserRegister");

        [HttpPost]
        public IActionResult Register(KhachHang kh)
        {
            if (_context.KhachHangs.Any(u => u.Email == kh.Email))
            {
                ViewBag.Error = "Email đã tồn tại";
                return View("NvbUserRegister");
            }
            kh.NgayTao = DateTime.Now;
            _context.KhachHangs.Add(kh);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult EditInfoUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.KhachHangs.Find(userId);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInfoUser(KhachHang model, IFormFile? fAvatar)
        {
            // Quan trọng: Xóa kiểm tra các trường không có trên form để IsValid = True
            ModelState.Remove("MatKhau");
            ModelState.Remove("NgayTao");
            ModelState.Remove("fAvatar");

            var user = await _context.KhachHangs.FindAsync(model.MaKhachHang);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Xử lý Upload ảnh
                if (fAvatar != null && fAvatar.Length > 0)
                {
                    string folderPath = Path.Combine(_env.WebRootPath, "images", "avatars");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(user.AnhDaiDien))
                    {
                        string oldPath = Path.Combine(folderPath, user.AnhDaiDien);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fAvatar.FileName);
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await fAvatar.CopyToAsync(fileStream);

                    }
                    user.AnhDaiDien = fileName;
                    // Cập nhật lại avatar trong session để Navbar thay đổi ngay lập tức
                    HttpContext.Session.SetString("UserAvatar", fileName);
                }

                // Cập nhật thông tin text
                user.HoTen = model.HoTen;
                user.SoDienThoai = model.SoDienThoai;
                user.DiaChi = model.DiaChi;
                user.Email = model.Email; // Thường Email là ReadOnly trên View

                // Cập nhật session tên
                HttpContext.Session.SetString("UserName", user.HoTen ?? "");

                _context.Update(user);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("UserAvatar", user.AnhDaiDien ?? "default-avatar.png");
                HttpContext.Session.SetString("UserName", user.HoTen ?? "");

                ViewBag.Success = "Cập nhật thông tin thành công!";
            }
            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}