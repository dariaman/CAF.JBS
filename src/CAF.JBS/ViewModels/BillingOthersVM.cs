using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class BillingOthersVM
    {
        public string BillingID { get; set; }
        public int policy_id { get; set; }
        public string PolicyNo { get; set; }
        public string description { get; set; }
        public DateTime? BillingDate { get; set; }
        public string BillingType { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsDownload { get; set; }
        public string Source_download { get; set; }
        public int BankIdDownload { get; set; }
        public int BankID_Source { get; set; }
        public bool IsClosed { get; set; }
        public string status_billing { get; set; }
        public DateTime? LastUploadDate { get; set; }
        public DateTime? paid_date { get; set; }
        public DateTime? cancel_date { get; set; }
        public int? Life21TranID { get; set; }
        public int ReceiptOtherID { get; set; }
        public int PaymentTransactionID { get; set; }

        public DateTime? DateCrt { get; set; }
    }
}
