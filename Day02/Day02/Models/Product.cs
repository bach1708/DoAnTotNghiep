using System;
using System.Collections.Generic;

namespace Day02.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Title { get; set; } = null!;

    public string? Subtitle { get; set; }

    public string? Description { get; set; }

    public string? Sku { get; set; }

    public decimal Price { get; set; }

    public bool IsDigital { get; set; }

    public int? PublisherId { get; set; }

    public int? AuthorId { get; set; }

    public DateOnly? PublishedDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Author? Author { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Inventory? Inventory { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
