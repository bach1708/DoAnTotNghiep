public class EditUserVM
{
    public int MaKhachHang { get; set; }
    public string? HoTen { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public string? Email { get; set; } // Chỉ dùng để hiển thị (readonly)
    public string? AnhDaiDienHienTai { get; set; }
    public IFormFile? AnhUpload { get; set; } // File ảnh mới người dùng chọn
}