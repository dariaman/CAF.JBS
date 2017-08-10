using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("quote")]
    public class Quote
    {
        [Key]
        [Required]
        public int quote_id { get; set; }
        public string quote_ref_no { get; set; }
        public int quote_premium_mode { get; set; }
        public string quote_payment_method { get; set; }
        public Decimal quote_regular_premium { get; set; }
        public Decimal quote_single_premium { get; set; }
        public int quote_duration { get; set; }
        public int quote_duration_days { get; set; }
        public Decimal quote_paper_print_fee { get; set; }
        public int quote_prospect_id { get; set; }
        public int quote_holder_id { get; set; }
        public int quote_main_coverage_id { get; set; }
    }
}
