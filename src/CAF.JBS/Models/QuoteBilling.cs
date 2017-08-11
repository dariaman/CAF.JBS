using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("quote_billing")]
    public class QuoteBilling
    {
        [Key]
        [Required]
        public int quote_id { get; set; }
        public int product_id { get; set; }
        public string ref_no { get; set; }
    }
}
