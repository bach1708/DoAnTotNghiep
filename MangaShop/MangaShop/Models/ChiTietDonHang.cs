using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class ChiTietDonHang
{
    public int MaChiTiet { get; set; }

    public int MaDonHang { get; set; }

    public int MaTruyen { get; set; }

    public int SoLuong { get; set; }

    public double DonGia { get; set; }

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    public virtual Truyen MaTruyenNavigation { get; set; } = null!;
}
