namespace MangaShop.Models.ViewModels
{
    public class ThanhVienCuaToiVM
    {
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }

        public double TongChiTieu { get; set; }
        public int SoDonHang { get; set; }

        public string HangThanhVien { get; set; } = "Chưa có";
        public double MucTieuTiepTheo { get; set; }      // mốc tiếp theo (1tr/5tr/10tr)
        public string HangTiepTheo { get; set; } = "";   // tên hạng tiếp theo
        public int PhanTramTienDo { get; set; }          // 0..100

        public List<DonHangLiteVM> DonHangGanDay { get; set; } = new();
    }

    public class DonHangLiteVM
    {
        public int MaDonHang { get; set; }
        public DateTime? NgayDat { get; set; }
        public double TongTien { get; set; }
        public string? TrangThai { get; set; }
    }
}
