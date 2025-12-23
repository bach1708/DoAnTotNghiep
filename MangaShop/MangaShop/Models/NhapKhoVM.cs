namespace MangaShop.Models.ViewModels
{
    public class NhapKhoVM
    {
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = null!;
        public string? AnhBia { get; set; }
        public int SoLuongTon { get; set; }
        public int SoLuongNhap { get; set; }
    }
}
