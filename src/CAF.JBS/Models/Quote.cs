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
        public string prospect_name { get; set; }
        public string POB { get; set; }
        public DateTime DOB { get; set; }
        public Boolean IsLaki { get; set; }
        public string mobile_phone { get; set; }
        public string email { get; set; }
        public Decimal sum_insured { get; set; }
        public int premium_mode { get; set; }
        public string payment_method { get; set; }
        public Decimal? regular_premium { get; set; }
        public Decimal? single_premium { get; set; }
        public Decimal? paper_print_fee { get; set; }
        public int? duration { get; set; }
        public int? duration_days { get; set; }
    }
}
