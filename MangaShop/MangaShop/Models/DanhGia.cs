using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class DanhGia
{
    public int MaDanhGia { get; set; }

    public int MaTruyen { get; set; }

    public int MaKhachHang { get; set; }

    public int? SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual KhachHang? MaKhachHangNavigation { get; set; } = null!;

    public virtual Truyen? MaTruyenNavigation { get; set; } = null!;
}
