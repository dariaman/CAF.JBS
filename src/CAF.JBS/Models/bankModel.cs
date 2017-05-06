using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("bank")]
    public class BankModel
    {
        [Required]
        [Key]
        public int bank_id { get; set; }

        [Required]
        public string bank_code { get; set; }
    }
}
