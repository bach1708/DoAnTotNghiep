using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MangaShop.Models
{
    [Table("TruyenImages")]
    public class TruyenImages
    {
        [Key]
        public int Id { get; set; }

        public int MaTruyen { get; set; }

        [Required]
        [StringLength(250)]
        public string Path { get; set; }

        public int DisplayOrder { get; set; }

        // Khai báo quan hệ ngược lại với bảng Truyen
        [ForeignKey("MaTruyen")]
        public virtual Truyen Truyen { get; set; }
    }
}