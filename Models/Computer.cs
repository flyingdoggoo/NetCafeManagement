using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNet.Models
{
    public enum ComputerStatus
    {
        Available,
        InUse,
        UnderMaintenance,
    }
    public class Computer
    {
        [Key]
        public int ID { get; set; }
        [Required(ErrorMessage = "Tên máy tính không được để trống")]
        public string Name { get; set; }
        [Required]
        public ComputerStatus Status { get; set; }
        [Required]
        [Range(5000, 100000, ErrorMessage = "Giá tiền không hợp lệ")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public virtual ICollection<Bill> Bills { get; set; }
    }
}
