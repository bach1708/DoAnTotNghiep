using Microsoft.AspNetCore.Mvc;
using MangaShop.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using System.Collections.Generic;
using MangaShop.Helpers; // ✅ Phải có dòng này để dùng GetObject

namespace MangaShop.Controllers
{
    public class NvbAccountController : Controller
    {
        private readonly MangaShopContext _context;

        public NvbAccountController(MangaShopContext context)
        {
            _context = context;
        }

        // ===== GET: Mở form đăng nhập USER =====
        [HttpGet]
        public IActionResult Login()
        {
            return View("NvbUserLogin");
        }

        // ===== POST: Xử lý đăng nhập USER =====
        [HttpPost]
        public IActionResult Login(string Email, string MatKhau)
        {
            var user = _context.KhachHangs
                .FirstOrDefault(u => u.Email == Email && u.MatKhau == MatKhau);

            if (user != null)
            {
                // 1. Lưu session user
                HttpContext.Session.SetInt32("UserId", user.MaKhachHang);
                HttpContext.Session.SetString("UserName", user.HoTen);

                // 2. ✅ THỰC HIỆN ĐỒNG BỘ GIỎ HÀNG TỪ SESSION VÀO DATABASE
                SyncCartToDb(user.MaKhachHang);

                return RedirectToAction("Home", "NvbHome");
            }

            ViewBag.Error = "Email hoặc mật khẩu không đúng";
            return View("NvbUserLogin");
        }

        // --- Hàm phụ xử lý đồng bộ giỏ hàng ---
        private void SyncCartToDb(int userId)
        {
            // 1. Lấy giỏ hàng tạm từ Session "CART"
            var sessionCart = HttpContext.Session.GetObject<List<CartItem>>("CART");

            // Nếu session không có hàng thì không cần làm gì cả
            if (sessionCart == null || !sessionCart.Any()) return;

            // 2. Tìm giỏ hàng của User này trong DB, nếu chưa có thì tạo mới
            var dbGioHang = _context.GioHangs.FirstOrDefault(g => g.MaKhachHang == userId);
            if (dbGioHang == null)
            {
                dbGioHang = new GioHang
                {
                    MaKhachHang = userId,
                    NgayTao = DateTime.Now
                };
                _context.GioHangs.Add(dbGioHang);
                _context.SaveChanges(); // Lưu để lấy MaGioHang vừa tạo
            }

            // 3. Duyệt từng món trong Session để đưa vào Database
            foreach (var item in sessionCart)
            {
                // Kiểm tra xem món này (theo MaTap) đã có trong DB của User này chưa
                var existingItem = _context.ChiTietGioHangs
                    .FirstOrDefault(ct => ct.MaGioHang == dbGioHang.MaGioHang && ct.MaTap == item.MaTap);

                if (existingItem != null)
                {
                    // Nếu đã có trong DB rồi thì cộng dồn số lượng
                    existingItem.SoLuong += item.SoLuong;
                }
                else
                {
                    // Nếu chưa có thì thêm mới dòng vào bảng ChiTietGioHang
                    _context.ChiTietGioHangs.Add(new ChiTietGioHang
                    {
                        MaGioHang = dbGioHang.MaGioHang,
                        MaTruyen = item.MaTruyen,
                        MaTap = item.MaTap, // Đảm bảo bạn đã chạy lệnh ALTER TABLE thêm cột này trong SQL
                        SoLuong = item.SoLuong
                    });
                }
            }

            // 4. Lưu thay đổi vào Database và XÓA Session CART
            _context.SaveChanges();
            HttpContext.Session.Remove("CART");
        }

        // ===== GET: Mở form đăng ký =====
        [HttpGet]
        public IActionResult Register()
        {
            return View("NvbUserRegister");
        }

        // ===== POST: Xử lý đăng ký =====
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

        // Sửa thông tin user
        [HttpGet]
        public IActionResult EditInfoUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.KhachHangs.Find(userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditInfoUser(KhachHang model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.KhachHangs.Find(userId);
            if (user == null)
                return NotFound();

            user.HoTen = model.HoTen;
            user.Email = model.Email;
            user.SoDienThoai = model.SoDienThoai;
            user.DiaChi = model.DiaChi;

            _context.SaveChanges();

            HttpContext.Session.SetString("UserName", user.HoTen);

            ViewBag.Success = "Cập nhật thông tin thành công";
            return View(user);
        }

        // ===== Đăng xuất USER =====
        public IActionResult Logout()
        {
            // Xóa hết session bao gồm cả User thông tin và giỏ hàng hiện tại (nếu có)
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}