using System;

namespace CAF.JBS.ViewModels
{
    public class StagingUploadVM
    {
        public int id { get; set; }        
        // data download
        public string polisNo { get; set; }
        public int? PolicyId { get; set; }
        public int? ReqSeq { get; set; }
        public string BillingID { get; set; }

        public string BillCode { get; set; }
        public DateTime? tgl { get; set; }
        public Decimal amount { get; set; }
        public Boolean IsSuccess { get; set; }
        public string StatusPolis { get; set; }

        // untuk data dari tabel billing/other/quote 
        public string BillType { get; set; }
        public Decimal? billAmount { get; set; }
        public Nullable<DateTime> Due_Date_Pre { get; set; }
        public string ApprovalCode { get; set; }
        public string PaymentSource { get; set; }
        public int BankidPaid { get; set; }
        public string Description { get; set; }
        public string ACCno { get; set; }
        public string ACCname { get; set; }
        public string CC_Expiry { get; set; }
        public string trancode { get; set; }
        public string filename { get; set; }
        public int? life21TranID { get; set; } // ada isi untuk billing others

        

        // Untuk kebutuhan temporer
        public DateTime TglSkrg { get; set; }
        public Decimal? CashlessFee { get; set; }
        public int receipt_id { get; set; }
        public int? receipt_other_id { get; set; }
        public int? PolisRefundId { get; set; }
        public int PaymentTransactionID { get; set; } // id di histori
        public string StatusBilling { get; set; } // status billing nantinya A,P,R
    }
}
