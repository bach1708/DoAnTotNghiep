    using System;

    namespace MangaShop.Models
    {
        public partial class LichPhatHanh
        {
            public int MaLich { get; set; }
            public int MaTruyen { get; set; }
            public int? MaTap { get; set; }
            public DateTime NgayPhatHanh { get; set; }
            public decimal? GiaDuKien { get; set; }
            public string? GhiChu { get; set; }
            public bool TrangThai { get; set; }
            public DateTime NgayTao { get; set; }
            public DateTime? NgayCapNhat { get; set; }

            public virtual Truyen? MaTruyenNavigation { get; set; } = null!;
            public virtual TruyenTap? MaTapNavigation { get; set; }
        }
    }
