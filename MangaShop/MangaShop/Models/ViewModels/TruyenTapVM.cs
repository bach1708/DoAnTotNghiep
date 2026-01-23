namespace MangaShop.Models.ViewModels
{
    public class ThemTapVM
    {
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } // Để hiển thị tên truyện cho dễ nhìn
        public int SoTapTiepTheo { get; set; } // Tự động tính toán (ví dụ: 24)
        public double Gia { get; set; }
        public int SoLuongThem { get; set; } // Số lượng nhập mới
    }
}
