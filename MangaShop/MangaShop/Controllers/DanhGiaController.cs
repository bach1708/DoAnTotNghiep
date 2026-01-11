using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangaShop.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly MangaShopContext _context;

        public DanhGiaController(MangaShopContext context)
        {
            _context = context;
        }

        // ===== GET: FORM ĐÁNH GIÁ =====
        [HttpGet]
        public IActionResult Create(int maTruyen, int maDonHang)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "NvbAccount");

            // ✅ Lấy danh sách tập đã mua trong đơn này (thuộc truyện này)
            var tapsDaMua = _context.ChiTietDonHangs
                .Include(ct => ct.MaDonHangNavigation)
                .Include(ct => ct.MaTapNavigation)
                .Where(ct =>
                    ct.MaDonHang == maDonHang &&
                    ct.MaDonHangNavigation.MaKhachHang == userId.Value &&
                    ct.MaTruyen == maTruyen &&
                    ct.MaTap != null)
                .Select(ct => ct.MaTapNavigation!)
                .Distinct()
                .OrderBy(t => t.SoTap)
                .ToList();

            ViewBag.MaTruyen = maTruyen;
            ViewBag.MaDonHang = maDonHang;
            ViewBag.TapsDaMua = tapsDaMua;

            return View();
        }

        // ===== POST: LƯU ĐÁNH GIÁ =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int MaTruyen, int MaDonHang, int SoSao, string? NoiDung, int[] maTaps)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "NvbAccount");

            // ✅ validate cơ bản
            if (SoSao < 1 || SoSao > 5)
            {
                TempData["Err"] = "Bạn chưa chọn số sao hợp lệ.";
                return RedirectToAction("Create", new { maTruyen = MaTruyen, maDonHang = MaDonHang });
            }

            if (maTaps == null || maTaps.Length == 0)
            {
                TempData["Err"] = "Bạn cần chọn ít nhất 1 tập đã mua để đánh giá.";
                return RedirectToAction("Create", new { maTruyen = MaTruyen, maDonHang = MaDonHang });
            }

            // ✅ Validate tập có thuộc đơn + thuộc user + thuộc truyện không
            var tapHopLe = _context.ChiTietDonHangs
                .Include(ct => ct.MaDonHangNavigation)
                .Where(ct =>
                    ct.MaDonHang == MaDonHang &&
                    ct.MaDonHangNavigation.MaKhachHang == userId.Value &&
                    ct.MaTruyen == MaTruyen &&
                    ct.MaTap != null)
                .Select(ct => ct.MaTap!.Value)
                .Distinct()
                .ToHashSet();

            foreach (var mt in maTaps.Distinct())
            {
                if (!tapHopLe.Contains(mt))
                {
                    TempData["Err"] = "Có tập không hợp lệ (không thuộc đơn/truyện).";
                    return RedirectToAction("Create", new { maTruyen = MaTruyen, maDonHang = MaDonHang });
                }
            }

            // ✅ Chặn trùng: đã đánh giá tập đó rồi thì bỏ qua
            var tapDaDanhGia = _context.DanhGias
                .Where(dg => dg.MaKhachHang == userId.Value && dg.MaTap != null)
                .Select(dg => dg.MaTap!.Value)
                .ToHashSet();

            var tapsCanLuu = maTaps.Distinct()
                                   .Where(mt => !tapDaDanhGia.Contains(mt))
                                   .ToList();

            if (!tapsCanLuu.Any())
            {
                TempData["Err"] = "Bạn đã đánh giá các tập này rồi.";
                return RedirectToAction("BillDetail", "Bill", new { id = MaDonHang });
            }

            foreach (var mt in tapsCanLuu)
            {
                _context.DanhGias.Add(new DanhGia
                {
                    MaTruyen = MaTruyen,
                    MaKhachHang = userId.Value,
                    MaTap = mt,
                    SoSao = SoSao,
                    NoiDung = NoiDung,
                    NgayDanhGia = DateTime.Now
                });
            }

            _context.SaveChanges();

            TempData["Ok"] = "Đã gửi đánh giá thành công.";
            return RedirectToAction("BillDetail", "Bill", new { id = MaDonHang });
        }
    }
}
