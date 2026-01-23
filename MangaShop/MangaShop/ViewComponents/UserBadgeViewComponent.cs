using MangaShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MangaShop.ViewComponents
{
    public class UserBadgeViewComponent : ViewComponent
    {
        private readonly MangaShopContext _context;

        public UserBadgeViewComponent(MangaShopContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // ✅ luôn trả về VM
            var vm = new UserBadgeVM
            {
                Tier = "Thường",
                TongChiTieu = 0
            };

            if (userId == null)
                return View("Default", vm);

            double tongChiTieu = _context.DonHangs
                .Where(d => d.MaKhachHang == userId.Value && d.TrangThai == "Hoàn thành")
                .Sum(d => (double?)d.TongTien) ?? 0;

            vm.TongChiTieu = tongChiTieu;

            // ✅ Mốc mới:
            // < 500k: Thường
            // >= 500k: Bạc
            // >= 2tr: Vàng
            // >= 5tr: Kim cương
            vm.Tier =
                tongChiTieu >= 5_000_000 ? "Kim cương" :
                tongChiTieu >= 2_000_000 ? "Vàng" :
                tongChiTieu >= 500_000 ? "Bạc" :
                "Thường";

            return View("Default", vm);
        }
    }

    public class UserBadgeVM
    {
        public string Tier { get; set; } = "Thường";
        public double TongChiTieu { get; set; }
    }
}
