using System;

namespace CAF.JBS.ViewModels
{
    public class PolicyTransaction
    {
        public int? idTran { get; set; }
        public int? policy_id { get; set; }
        public DateTime transaction_dt { get; set; }
        public string transaction_type { get; set; }
        public int? recurring_seq { get; set; }
        public decimal amount { get; set; }
        public Nullable<DateTime> Due_Date_Pre { get; set; }
        public int BankID { get; set; }
        public string ACC_No { get; set; }
        public string result_status { get; set; }
        public string Remark { get; set; }
        public int receipt_id { get; set; }
        public int receipt_other_id { get; set; }
        public DateTime update_dt { get; set; }

    }
}
