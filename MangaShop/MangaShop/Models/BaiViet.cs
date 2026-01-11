using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class BaiViet
{
    public int MaBaiViet { get; set; }
    public string TieuDe { get; set; } = null!;
    public string? TomTat { get; set; }
    public string NoiDung { get; set; } = null!;
    public string? AnhDaiDien { get; set; }
    public string Loai { get; set; } = "Thông báo";
    public DateTime NgayDang { get; set; }
    public bool TrangThai { get; set; } = true; // true=hiện
}
