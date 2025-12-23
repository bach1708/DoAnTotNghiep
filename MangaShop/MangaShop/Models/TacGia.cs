using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class TacGia
{
    public int MaTacGia { get; set; }

    public string TenTacGia { get; set; } = null!;

    public string? GioiThieu { get; set; }

    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
