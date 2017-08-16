using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("stagingupload")]
    public class StagingUpload
    {
        [Key]
        public int id { get; set; }
        public string polisNo { get; set; }
        public string BillCode { get; set; }
        public Nullable<DateTime> tgl { get; set; } // utk tgl transaksi >> lebih krusial utk VA
        public Nullable<DateTime> due_dt_pre { get; set; }
        public Decimal amount { get; set; } // total amount dari file Upload
        public Boolean IsSuccess { get; set; }
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
        public int? PolicyId { get; set; }
        public int? recurring_seq { get; set; }
        public string Billid { get; set; }

        // Untuk kebutuhan temporer
        public DateTime TglSkrg { get; set; }
        public Decimal? CashlessFee { get; set; }
        public string BillType { get; set; }
        public int receipt_id { get; set; }
        public int? receipt_other_id { get; set; }
        public int PaymentTransactionID { get; set; } // id di histori
    }
}
