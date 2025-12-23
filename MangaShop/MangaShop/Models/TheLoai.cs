using System;
using System.Collections.Generic;

namespace MangaShop.Models;

public partial class TheLoai
{
    public int MaTheLoai { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public virtual ICollection<Truyen> Truyens { get; set; } = new List<Truyen>();
}
