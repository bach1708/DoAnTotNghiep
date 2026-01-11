using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class QuanTri
{
    public int MaAdmin { get; set; }

    public string TaiKhoan { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? HoTen { get; set; }

    public bool? TrangThai { get; set; }
    public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; } = new HashSet<PhieuNhap>();

}
