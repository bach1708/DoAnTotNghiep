namespace MangaShop.Models
{
    public class CartItem
    {
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = null!;
        public string? AnhBia { get; set; }
        public double Gia { get; set; }
        public int SoLuong { get; set; }
        public int MaTap { get; set; }   // NEW
        public int SoTap { get; set; }   // NEW

        // Thành tiền
        public double ThanhTien => Gia * SoLuong;
    }
}
