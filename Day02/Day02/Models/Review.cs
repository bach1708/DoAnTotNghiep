using System;
using System.Collections.Generic;

namespace Day02.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    public int? UserId { get; set; }

    public byte Rating { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsApproved { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User? User { get; set; }
}
