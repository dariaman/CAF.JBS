using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class TransactionBank
    {
        public string File_Backup { get; set; }
        public string TranCode { get; set; }
        public DateTime? TranDate { get; set; }
        public Boolean IsSuccess { get; set; }
        public int? PolicyId { get; set; }
        public string BillingID { get; set; }
        public Decimal BillAmount { get; set; }
        public string ApprovalCode { get; set; }
        public string deskripsi { get; set; }
        public string accNo { get; set; }
    }
}
