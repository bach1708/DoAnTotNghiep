using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class NhaXuatBan
{
    public int MaNxb { get; set; }

    public string TenNxb { get; set; } = null!;

    public string? DiaChi { get; set; }

    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
