using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("receipt_other")]
    public class receiptOtherModel
    {
        [Required]
        [Key]
        public int receipt_other_id { get; set; }
        public DateTime receipt_date { get; set; }
        public int policy_id { get; set; }
        public int receipt_type_id { get; set; }
        public Decimal receipt_amount { get; set; }
        public string receipt_source { get; set; }
        public DateTime receipt_payment_date { get; set; }
        public int receipt_seq { get; set; }
        public int bank_acc_id { get; set; }
        public int acquirer_bank_id { get; set; }
        public string created_by { get; set; }
        public int receipt_id { get; set; }
    }
}
