using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("prefixcard")]
    public class prefixcardModel
    {
        [Required]
        [Key]
        public int Prefix { get; set; }

        [Required]
        public int bank_id { get; set; }

        public int Type { get; set; }
        public string Description { get; set; }
    }
}
