using System;
using System.Collections.Generic;

namespace Day02.Models;

public partial class Author
{
    public int AuthorId { get; set; }

    public string Name { get; set; } = null!;

    public string? Bio { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
