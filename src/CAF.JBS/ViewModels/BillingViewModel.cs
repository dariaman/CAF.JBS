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
        public string policy_id { get; set; }
        public string PolicyNo { get; set; }
        public string payment_method { get; set; }
        public string recurring_seq { get; set; }
        public DateTime? BillingDate { get; set; }
        public DateTime due_dt_pre { get; set; }

        public Decimal policy_regular_premium { get; set; }
        public Decimal cashless_fee_amount { get; set; }
        public Decimal TotalAmount { get; set; }

        public string status_billing { get; set; }
        public Boolean IsHold { get; set; }
        public string PaymentSource { get; set; }
        public DateTime? paid_date { get; set; }
        public DateTime? cancel_date { get; set; }
        public DateTime? LastUploadDate { get; set; }
        
        public DateTime? DateCrt { get; set; }

    }
}
