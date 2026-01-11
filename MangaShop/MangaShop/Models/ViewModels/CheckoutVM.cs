using MangaShop.Models;
using System.Collections.Generic;

namespace MangaShop.Models.ViewModels
{
    public class CheckoutVM
    {
        public List<CartItem> Cart { get; set; } = new();

        // Thông tin người mua (cho phép sửa trên form)
        public string HoTen { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string? DiaChi { get; set; }

        // Phương thức thanh toán
        public string PaymentMethod { get; set; } = "COD"; // mặc định
    }
}
