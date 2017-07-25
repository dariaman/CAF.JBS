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

        public Decimal? BillingAmountDWD { get; set; }
        public Decimal? OthersAmountDWD { get; set; }
        public Decimal? QuoteAmountDWD { get; set; }
        public Decimal? TotalAmountDWD { get; set; }

        public int? BillingCountDWD { get; set; }
        public int? OthersCountDWD { get; set; }
        public int? QuoteCountDWD { get; set; }
        public int? TotalCountDWD { get; set; }

        public string Description { get; set; }
    }
}
