using System;
using System.Collections.Generic;

namespace MangaShop.Models
{
    public partial class TruyenTap
    {
        public int MaTap { get; set; }
        public int MaTruyen { get; set; }
        public int SoTap { get; set; }
        public double Gia { get; set; }
        public int SoLuongTon { get; set; }

        public virtual Truyen MaTruyenNavigation { get; set; } = null!;
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new HashSet<ChiTietPhieuNhap>();
        public virtual ICollection<LichPhatHanh> LichPhatHanhs { get; set; } = new List<LichPhatHanh>();


    }
}
