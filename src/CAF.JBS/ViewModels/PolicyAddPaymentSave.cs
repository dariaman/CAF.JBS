using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class PolicyAddPaymentSave
    {
        public Int32 PolicyId { get; set; }
        public Int32? BillingID { get; set; }
        
        public DateTime? BillingDate { get; set; }
        public DateTime PaidDate { get; set; }
        public String SourcePayment { get; set; }

        public Decimal Premi { get; set; }
        public Decimal CashLess { get; set; }
        public Decimal PaidAmount { get; set; }
    }
}
