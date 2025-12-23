using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class GioHang
{
    public int MaGioHang { get; set; }

    public int MaKhachHang { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual KhachHang MaKhachHangNavigation { get; set; } = null!;
}
