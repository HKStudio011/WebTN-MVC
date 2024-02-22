using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTN_MVC.Models.Contact
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        [Required(ErrorMessage = "Phải nhập {0}.")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phải nhập {0}.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Phải đúng định dạng {0}.")]
        public string Email { get; set; }

        public DateTime DateSent { get; set; }

        [Required(ErrorMessage = "Phải nhập {0}.")]
        [Display(Name = "Nội dung")]
        public string Message { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "Phải đúng định dạng {0}.")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }
    }
}