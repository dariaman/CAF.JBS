using System;

namespace CAF.JBS.ViewModels
{
    public class Receipt
    {
        public DateTime receipt_date { get; set; }
        public int? receipt_policy_id { get; set; }
        public Decimal receipt_amount { get; set; }
        public string receipt_source { get; set; }
        public int? receipt_seq { get; set; }
        public int bank_acc_id { get; set; }
        public Nullable<DateTime> due_date_pre { get; set; }
    }
}
