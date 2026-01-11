using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class DonHang
{
    public int MaDonHang { get; set; }

    public int MaKhachHang { get; set; }

    public DateTime? NgayDat { get; set; }

    public double TongTien { get; set; }

    public string? TrangThai { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual KhachHang MaKhachHangNavigation { get; set; } = null!;
}
