using System;
using System.ComponentModel.DataAnnotations;

namespace CAF.JBS.ViewModels
{
    public class PolicyBillingViewModel
    {
        [Required]
        [Key]
        public int policy_Id { get; set; }
        public string policy_no { get; set; }
        public string payment_method { get; set; }
        public DateTime commence_dt { get; set; }
        public DateTime due_dt { get; set; }
        public int premium_mode { get; set; }
        public DateTime due_dt_pre { get; set; }
        public int product_id { get; set; }
        public string product_code { get; set; }
        public int holder_id { get; set; }
        public string HolderName { get; set; }
        public string EmailHolder { get; set; }
        public decimal regular_premium { get; set; }
        public string Policy_status { get; set; }
        public string cc_no { get; set; }
        public int? cc_acquirer_bank_id { get; set; }
        public string cc_expiry { get; set; }
        public string cc_name { get; set; }
        public string cc_address { get; set; }
        public string cc_telephone { get; set; }
        public string acc_no { get; set; }
        public int? acc_bank_id { get; set; }
        public string acc_name { get; set; }
        public string acc_bank_branch { get; set; }
        public string VANo { get; set; }
        public string VAName { get; set; }
        public int? last_recurring_seq { get; set; }
        public string last_payment_source { get; set; }
        public int? last_receipt_id { get; set; }
        public int? last_acquirer_bank_id { get; set; }
        public bool IsHoldBilling { get; set; }
        public Nullable<DateTime> last_receipt_date { get; set; }
        public string UserCrt { get; set; }
        public DateTime DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime DateUpdate { get; set; }
    }
}
