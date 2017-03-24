using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    public class BillingModel
    {
        public string BillingID { get; set; }
        public int policy_id { get; set; }
        public int recurring_seq { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime due_date_pre { get; set; }
        public string PeriodeBilling { get; set; }
        public string BillingType { get; set; }
        public decimal policy_regular_premium { get; set; }
        public string DISC_REGULAR_PREMIUM { get; set; }
        public string DISC_REGULAR_PREMIUM_PCT_Amount { get; set; }
        public string TotalAmount { get; set; }
        public string statusBilling { get; set; }
        public string IsDownload { get; set; }
        public DateTime DownloadDate { get; set; }
        public string ReceiptID { get; set; }
        public string PaymentTransactionID { get; set; }
        public string UserCrt { get; set; }
        public DateTime DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime DateUpdate { get; set; }
    }
}
