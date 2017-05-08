using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class BillingViewModel
    {
        [Key]
        public string BillingID { get; set; }
        public int policy_id { get; set; }
        public string PolicyNo { get; set; }
        public int recurring_seq { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<DateTime> BillingDate { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime due_date_pre { get; set; }
        public string PeriodeBilling { get; set; }
        public string BillingType { get; set; }
        public decimal policy_regular_premium { get; set; }
        public string DISC_REGULAR_PREMIUM { get; set; }
        public string DISC_REGULAR_PREMIUM_PCT_Amount { get; set; }
        public string TotalAmount { get; set; }
        public string status_billing { get; set; }
        public string IsDownload { get; set; }
        public int? BankIdDownload { get; set; }
        public string ReceiptID { get; set; }
        public string PaymentTransactionID { get; set; }
        public string UserCrt { get; set; }
        public DateTime DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime DateUpdate { get; set; }

    }
}
