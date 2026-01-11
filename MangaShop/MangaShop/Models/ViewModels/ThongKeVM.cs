namespace MangaShop.Models.ViewModels
{
    public class DoanhThuTheoNgayVM
    {
        public DateTime Ngay { get; set; }
        public double DoanhThu { get; set; }
        public int SoDon { get; set; }
    }

    public class TopTruyenVM
    {
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = "";
        public int TongSoLuong { get; set; }
        public double DoanhThu { get; set; }
    }

    public class ThongKeVM
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }

        public double TongDoanhThu { get; set; }
        public int TongDon { get; set; }
        public int DonHoanThanh { get; set; }
        public int DonHuy { get; set; }

        public List<DoanhThuTheoNgayVM> DoanhThuTheoNgay { get; set; } = new();
        public List<TopTruyenVM> TopTruyen { get; set; } = new();
    }
}
