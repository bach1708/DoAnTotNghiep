using System;
using System.Collections.Generic;

namespace Day02.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? PaymentProviderRef { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaidAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
