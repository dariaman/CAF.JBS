using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class PolicyAddPayment
    {
        public string PolicyId { get; set; }
        public string policy_no { get; set; }
        public string StatusPolis { get; set; }
        public DateTime CommenceDate { get; set; }
        public DateTime DueDate { get; set; }

        public string ProductDesc { get; set; }
        public string PremiumMode { get; set; }

        public string HolderName { get; set; }
        public Decimal PaidAmount { get; set; }
        public string SourcePayment { get; set; }
        public string BillingID { get; set; }
        public DateTime Due_date_pre { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime PaidDate { get; set; }
        public String ApprovalCode { get; set; }

        public Decimal Premi { get; set; }
        public Decimal CashLess { get; set; }

    }
}
