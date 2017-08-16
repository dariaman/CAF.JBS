using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("billing_others")]
    public class BillingOtherModel
    {
        [Key]
        public string BillingID { get; set; }
        public int policy_id { get; set; }
        public string description { get; set; }
        public DateTime? BillingDate { get; set; }
        public string BillingType { get; set; }
        public Decimal TotalAmount { get; set; }
        public Boolean IsDownload { get; set; }
        public string Source_download { get; set; }
        public int BankIdDownload { get; set; }
        public int BankID_Source { get; set; }
        public Boolean IsClosed { get; set; }
        public string status_billing { get; set; }
        public DateTime? LastUploadDate { get; set; }
        public string UserUpload { get; set; }
        public string PaymentSource { get; set; }
        public int? BankIdPaid { get; set; }
        public DateTime? paid_date { get; set; }
        public Decimal PaidAmount { get; set; }
        public DateTime? cancel_date { get; set; }
        public int Life21TranID { get; set; }
        public int? ReceiptOtherID { get; set; }
        public int? PaymentTransactionID { get; set; }
        public string AccNo { get; set; }
        public string AccName { get; set; }
        public string cc_expiry { get; set; }
    }
}
