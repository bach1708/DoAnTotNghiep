using System;
using System.Collections.Generic;

namespace MangaShop.Models
{
    public partial class PhieuNhap
    {
        public PhieuNhap()
        {
            ChiTietPhieuNhaps = new HashSet<ChiTietPhieuNhap>();
        }

        public int MaPhieuNhap { get; set; }
        public int MaAdmin { get; set; }
        public DateTime NgayNhap { get; set; }
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = null!;
        public decimal? TongTienNhap { get; set; }

        // Navigation
        public virtual QuanTri MaAdminNavigation { get; set; } = null!;
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
    }
}
