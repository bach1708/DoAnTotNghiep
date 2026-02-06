using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
    public int? DinhDang { get; set; } // 0: Truyện ngắn, 1: Truyện dài

    public int? MaTacGia { get; set; }

    public int? MaNxb { get; set; }
    public int SoLuongTon { get; set; }

    [Display(Name = "Loại truyện")]
    public int LoaiTruyen { get; set; } // 0: Truyện Tranh, 1: Truyện Chữ
    public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new HashSet<ChiTietPhieuNhap>();


    public virtual ICollection<TruyenTap> TruyenTaps { get; set; } = new List<TruyenTap>();

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();

    public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();

    public virtual NhaXuatBan? MaNxbNavigation { get; set; }

    public virtual TacGia? MaTacGiaNavigation { get; set; }

    public virtual TheLoai? MaTheLoaiNavigation { get; set; }
    public virtual ICollection<LichPhatHanh> LichPhatHanhs { get; set; } = new List<LichPhatHanh>();
    public virtual ICollection<TruyenImages> TruyenImages { get; set; } = new List<TruyenImages>();

    public bool IsDeleted { get; set; }

}
