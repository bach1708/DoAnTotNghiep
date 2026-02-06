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
        public async Task<IActionResult> EditInfoUser(KhachHang model, IFormFile? fAvatar, string? newPassword)
        {
            // Xóa validation các trường không có trong form
            ModelState.Remove("MatKhau");
            ModelState.Remove("NgayTao");
            ModelState.Remove("fAvatar");
            ModelState.Remove("newPassword");

            var user = await _context.KhachHangs.FindAsync(model.MaKhachHang);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                // 1. Xử lý ảnh đại diện (giữ nguyên logic cũ của bạn)
                if (fAvatar != null && fAvatar.Length > 0)
                {
                    string folderPath = Path.Combine(_env.WebRootPath, "images", "avatars");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    if (!string.IsNullOrEmpty(user.AnhDaiDien))
                    {
                        string oldPath = Path.Combine(folderPath, user.AnhDaiDien);
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fAvatar.FileName);
                    using (var stream = new FileStream(Path.Combine(folderPath, fileName), FileMode.Create))
                    {
                        await fAvatar.CopyToAsync(stream);
                    }
                    user.AnhDaiDien = fileName;
                    HttpContext.Session.SetString("UserAvatar", fileName);
                }

                // 2. Cập nhật thông tin cơ bản
                user.HoTen = model.HoTen;
                user.SoDienThoai = model.SoDienThoai;
                user.DiaChi = model.DiaChi;

                // 3. Cập nhật mật khẩu nếu có thay đổi từ input newPassword
                if (!string.IsNullOrEmpty(newPassword) && newPassword != user.MatKhau)
                {
                    user.MatKhau = newPassword;
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UserName", user.HoTen ?? "");
                ViewBag.Success = "Cập nhật hồ sơ thành công!";
            }
            return View(user);
        }
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TimKiemTaiKhoan(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return Json(new { success = false });

            // BƯỚC 1: Chỉ tìm tài khoản bằng EMAIL
            var kh = _context.KhachHangs.FirstOrDefault(u => u.Email == keyword);

            if (kh != null)
            {
                return Json(new
                {
                    success = true,
                    maskedP = MaskPhone(kh.SoDienThoai) // Hiện SĐT đã che để gợi nhớ
                });
            }
            return Json(new { success = false, message = "Email này chưa được đăng ký tài khoản!" });
        }

        [HttpPost]
        public IActionResult CapNhatMatKhau(string account, string phone, string newPass)
        {
            // account ở đây sẽ là Email khách đã nhập ở bước 1
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(newPass))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin!" });
            }

            // BƯỚC 2: Tìm khách hàng có Email đó VÀ có Số điện thoại khớp với số khách vừa nhập
            var kh = _context.KhachHangs.FirstOrDefault(u => u.Email == account && u.SoDienThoai == phone);

            if (kh != null)
            {
                if (newPass == "CHECK_ONLY") return Json(new { success = true });

                // BƯỚC 3: Cập nhật mật khẩu mới
                kh.MatKhau = newPass;
                _context.SaveChanges();
                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }

            return Json(new { success = false, message = "Số điện thoại xác nhận không chính xác!" });
        }

        // --- CÁC HÀM HELPER ĐÃ SỬA LỖI RETURN ---

        private string MaskEmail(string e)
        {
            if (string.IsNullOrEmpty(e) || !e.Contains("@"))
                return "********"; // Trả về mặc định nếu không phải email

            var parts = e.Split('@');
            var name = parts[0];
            var domain = parts[1];

            if (name.Length <= 2)
                return name.Substring(0, 1) + "****@" + domain;

            // Lấy ký tự đầu và cuối của phần tên, ở giữa thay bằng ****
            return name.Substring(0, 1) + "****" + name.Substring(name.Length - 1) + "@" + domain;
        }

        private string MaskPhone(string p)
        {
            if (string.IsNullOrEmpty(p) || p.Length < 3)
                return "********"; // Trả về mặc định nếu SĐT quá ngắn

            // Lấy ký tự đầu và cuối, ở giữa là 7 dấu sao
            return p.Substring(0, 1) + "*******" + p.Substring(p.Length - 1);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}