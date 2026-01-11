using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MangaShop.Models.ViewModels
{
    public class BaiVietFormVM
    {
        public int MaBaiViet { get; set; }

        [Required(ErrorMessage = "Nhập tiêu đề")]
        [StringLength(200)]
        public string TieuDe { get; set; } = "";

        [StringLength(500)]
        public string? TomTat { get; set; }

        [Required(ErrorMessage = "Nhập nội dung")]
        public string NoiDung { get; set; } = "";

        [Required]
        public string Loai { get; set; } = "Thông báo"; // Thông báo / Ưu đãi

        public bool TrangThai { get; set; } = true;

        // Upload ảnh
        public IFormFile? AnhUpload { get; set; }

        // Ảnh cũ (khi edit)
        public string? AnhDaiDienHienTai { get; set; }
    }
}
