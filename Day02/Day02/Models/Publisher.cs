using System;
using System.Collections.Generic;

namespace Day02.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string Name { get; set; } = null!;

    public string? Website { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
