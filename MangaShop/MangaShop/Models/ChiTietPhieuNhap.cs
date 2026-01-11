using System;

namespace MangaShop.Models
{
    public partial class ChiTietPhieuNhap
    {
        public int MaChiTietPhieuNhap { get; set; }
        public int MaPhieuNhap { get; set; }
        public int MaTruyen { get; set; }
        public int MaTap { get; set; }
        public int SoLuongNhap { get; set; }
        public decimal DonGiaNhap { get; set; }

        // Nếu trong DB bạn có cột computed "ThanhTien" thì có thể thêm:
        // public decimal ThanhTien { get; private set; }

        // Navigation
        public virtual PhieuNhap MaPhieuNhapNavigation { get; set; } = null!;
        public virtual Truyen MaTruyenNavigation { get; set; } = null!;
        public virtual TruyenTap MaTapNavigation { get; set; } = null!;
    }
}
