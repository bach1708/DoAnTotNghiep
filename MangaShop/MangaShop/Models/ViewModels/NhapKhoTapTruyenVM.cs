using System.Collections.Generic;

namespace MangaShop.Models.ViewModels
{
    public class NhapKhoTapTruyenVM
    {
        public int MaTruyen { get; set; }
        public string TenTruyen { get; set; } = "";
        public string? AnhBia { get; set; }

        public List<NhapKhoTapItemVM> Taps { get; set; } = new();
    }
}
