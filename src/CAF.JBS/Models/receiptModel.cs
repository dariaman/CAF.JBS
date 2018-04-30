using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("receipt")]
    public class receiptModel
    {
        [Required]
        [Key]
        public int receipt_id { get; set; }

        public DateTime receipt_date { get; set; }
        public int receipt_policy_id { get; set; }
        public int receipt_fund_type_id { get; set; }
        public string receipt_transaction_code { get; set; }
        public Decimal receipt_amount { get; set; }
        public string receipt_source { get; set; }
        public string receipt_status { get; set; }
        public DateTime receipt_payment_date_time { get; set; }
        public DateTime receipt_date_processed { get; set; }
        public int receipt_seq { get; set; }
        public int bank_acc_id { get; set; }
        public DateTime due_date_pre { get; set; }
        public int acquirer_bank_id { get; set; }
        public int? freq_payment { get; set; }

        public DateTime? created_date { get; set; }
        public String created_by { get; set; }
    }
}
