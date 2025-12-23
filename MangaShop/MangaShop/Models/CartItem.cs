namespace MangaShop.Models
{
    public class CartItem
    {
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = null!;
        public string? AnhBia { get; set; }
        public double Gia { get; set; }
        public int SoLuong { get; set; }

        // Thành tiền
        public double ThanhTien => Gia * SoLuong;
    }
}
