using System.ComponentModel.DataAnnotations;

namespace QuanLyNet.Models
{
    public class User
    {
        public int Id { get; set; }
        //Tên đăng nhập ít nhất 5 ký tự
        [MinLength(5, ErrorMessage ="Tên đăng nhập có ít nhất 5 ký tự")]
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ cái và số")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(5, ErrorMessage = "Mật khẩu có ít nhất 5 ký tự")]
        [StringLength(50, ErrorMessage = "Mật khẩu không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Mật khẩu chỉ được chứa chữ cái và số")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Tên người dùng không được để trống")]
        [StringLength(50, ErrorMessage = "Tên người dùng không được vượt quá 50 ký tự")]
        [MinLength(2, ErrorMessage = "Tên người dùng không hợp lệ")]
        public string Name { get; set; }
        //[Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        //[RegularExpression(@"^\d{10,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }
        public decimal Balance { get; set; }
        public bool isAdmin { get; set; } = false;
        public virtual ICollection<Bill>? Bills { get; set; }
    }
}
