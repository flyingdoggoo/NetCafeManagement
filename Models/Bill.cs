using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNet.Models
{
    public class Bill
    {
        public int ID { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public int ComputerID { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? TotalCost { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
        [ForeignKey("ComputerID")]
        public virtual Computer? Computer { get; set; }
    }
}
