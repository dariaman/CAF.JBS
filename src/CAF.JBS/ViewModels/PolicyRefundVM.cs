using System;

namespace CAF.JBS.ViewModels
{
    public class PolicyRefundVM
    {
        public int PolicyId { get; set; }
        public DateTime commenceDate { get; set; }
        public Decimal regularPremium { get; set; }
        public int refundType { get; set; }
        public DateTime refundDate { get; set; }
        public Decimal totalAmount { get; set; }
        public Decimal singlePremium { get; set; }
        public int receiptId { get; set; }
        public int receiptOtherId { get; set; }
    }
}
