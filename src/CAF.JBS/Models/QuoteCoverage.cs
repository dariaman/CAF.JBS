using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("quote_coverage")]
    public class QuoteCoverage
    {
        [Key]
        [Required]
        public int quote_coverage_id { get; set; }
        public int quote_id { get; set; }
        public decimal sum_insured { get; set; }
        //public int quote_coverage_id { get; set; }
        //public int quote_coverage_id { get; set; }
    }
}
