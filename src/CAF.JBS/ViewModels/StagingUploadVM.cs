using System;

namespace CAF.JBS.ViewModels
{
    public class StagingUploadVM
    {
        public int id { get; set; }
        
        // data download
        public string polisNo { get; set; }
        public string BillCode { get; set; }
        public Nullable<DateTime> tgl { get; set; }
        public Decimal amount { get; set; }
        public Boolean IsSuccess { get; set; }

        // untuk data dari tabel billing/other/quote
        public string policy_id { get; set; }
        public string BillingID { get; set; }
        public int? ReqSeq { get; set; }
        public Decimal? billAmount { get; set; }
        public Nullable<DateTime> Due_Date_Pre { get; set; }
    }
}
