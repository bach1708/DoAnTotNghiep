using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class Truyen
{
    public int MaTruyen { get; set; }

    public string TenTruyen { get; set; } = null!;

    public string? TacGia { get; set; }

    public double Gia { get; set; }

    public string? MoTa { get; set; }

    public string? AnhBia { get; set; }

    public int? MaTheLoai { get; set; }

    public int? MaTacGia { get; set; }

    public int? MaNxb { get; set; }
    public int SoLuongTon { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();

    public virtual NhaXuatBan? MaNxbNavigation { get; set; }

    public virtual TacGia? MaTacGiaNavigation { get; set; }

    public virtual TheLoai? MaTheLoaiNavigation { get; set; }
}
