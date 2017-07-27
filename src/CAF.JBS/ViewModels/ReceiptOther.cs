using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class ReceiptOther
    {
        public int? policy_id { get; set; }
        public Nullable<DateTime> receipt_date { get; set; }
        public Decimal? receipt_amount { get; set; }
        public string receipt_source { get; set; }
        public int? bank_acc_id { get; set; }
    }
}
