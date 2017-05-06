using System;
using System.ComponentModel.DataAnnotations;

namespace CAF.JBS.Models
{
    public class PolicyBillingModel
    {
        [Required]
        [Key]
        public uint policy_Id { get; set; }
        public string policy_no { get; set; }
        public string payment_method { get; set; }
        public DateTime commence_dt { get; set; }
        public DateTime due_dt { get; set; }
        public ushort premium_mode { get; set; }
        public DateTime due_dt_pre { get; set; }
        public uint  product_id { get; set; }
        public string product_code { get; set; }
        public uint holder_id { get; set; }
        public string HolderName { get; set; }
        public string EmailHolder { get; set; }
        public decimal regular_premium { get; set; }
        public string Policy_status { get; set; }
        public string cc_no { get; set; }
        public uint cc_acquirer_bank_id { get; set; }
        public string cc_expiry { get; set; }
        public string cc_name { get; set; }
        public string cc_address { get; set; }
        public string cc_telephone { get; set; }
        public string acc_no { get; set; }
        public uint acc_bank_id { get; set; }
        public string acc_name { get; set; }
        public string acc_bank_branch { get; set; }
        public string VANo { get; set; }
        public string VAName { get; set; }
        public ushort last_recurring_seq { get; set; }
        public string last_payment_source { get; set; }
        public uint last_receipt_id { get; set; }
        public uint last_acquirer_bank_id { get; set; }
        public DateTime last_receipt_date { get; set; }
        public string UserCrt { get; set; }
        public DateTime DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime DateUpdate { get; set; }
    }
}
