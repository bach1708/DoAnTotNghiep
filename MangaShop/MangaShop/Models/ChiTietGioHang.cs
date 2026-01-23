using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class ChiTietGioHang
{
    public int MaChiTiet { get; set; }

    public int MaGioHang { get; set; }

    public int MaTruyen { get; set; }

    public int? MaTap { get; set; }

    public int SoLuong { get; set; }

    public virtual GioHang MaGioHangNavigation { get; set; } = null!;

    public virtual Truyen MaTruyenNavigation { get; set; } = null!;

    public virtual TruyenTap? MaTapNavigation { get; set; }
}
