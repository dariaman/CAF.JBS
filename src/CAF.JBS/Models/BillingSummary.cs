using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("billing_download_summary")]
    public class BillingSummary
    {
        [Key]
        public int id { get; set; }

        public int BankID { get; set; }
        public int BankIDSource { get; set; }
        public string SourceDownload { get; set; }
        public string Judul { get; set; }
        public Decimal? AmountDownload { get; set; }
        public int? rowCountDownload { get; set; }
        public string Description { get; set; }
    }
}
