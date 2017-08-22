using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("billing")]
    public class BillingModel
    {
        [Key]
        public int BillingID { get; set; }
        public int policy_id { get; set; }
        public int recurring_seq { get; set; }
        public Nullable<DateTime> BillingDate { get; set; }
        public DateTime due_dt_pre { get; set; }
        public string PeriodeBilling { get; set; }
        public string BillingType { get; set; }
        public Decimal policy_regular_premium { get; set; }
        public Decimal cashless_fee_amount { get; set; }
        public Decimal TotalAmount { get; set; }
        public string status_billing { get; set; }
        public DateTime? cancel_date { get; set; }
        public Decimal? PaidAmount { get; set; }
        public Nullable<DateTime> paid_date { get; set; }
        public bool IsDownload { get; set; }
        public bool IsPending { get; set; }
        public bool IsClosed { get; set; }
        public string Source_download { get; set; }
        public string PaymentSource { get; set; }
        public int? BankIdDownload { get; set; }
        public int? ReceiptID { get; set; }
        public int? ReceiptOtherID { get; set; } // untuk transaksi cashless
        public int? PaymentTransactionID { get; set; } // id transaction_bank di JBS
        public int? Life21TranID { get; set; } // id CC/AC transaction di Life21
        public string UserCrt { get; set; }
        public string AccName { get; set; }
        public string AccNo { get; set; }
        public string cc_expiry { get; set; }
        public DateTime DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public Nullable<DateTime> DateUpdate { get; set; }
    }
}
