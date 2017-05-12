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
        public string BillingID { get; set; }
        public int policy_id { get; set; }
        public int recurring_seq { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime due_dt_pre { get; set; }
        public string PeriodeBilling { get; set; }
        public string BillingType { get; set; }
        public Decimal policy_regular_premium { get; set; }
        public Decimal TotalAmount { get; set; }
        public string status_billing { get; set; }
        public DateTime status_billing_dateUpdate { get; set; }
        public DateTime paid_date { get; set; }
        public Boolean IsDownload { get; set; }
        public int BankIdDownload { get; set; }
        public int ReceiptID { get; set; }
        public int PaymentTransactionID { get; set; }
        public string UserCrt { get; set; }
        public DateTime DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime DateUpdate { get; set; }
    }
}
