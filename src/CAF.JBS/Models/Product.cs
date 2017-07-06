using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("product")]
    public class Product
    {
        [Required]
        [Key]
        public int product_id { get; set; }
        public string product_code { get; set; }
        public string product_description { get; set; }
        public bool auto_renewal { get; set; }
    }
}
