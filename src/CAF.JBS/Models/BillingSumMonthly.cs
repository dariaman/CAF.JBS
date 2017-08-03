using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("billing_sum_monthly")]
    public class BillingSumMonthly
    {
        [Key, Column(Order = 0)]

        public int TranCode { get; set; }
        [Key, Column(Order = 1)]
        public string Periode { get; set; }
        public int PaidCount { get; set; }
        public Decimal PaidAmount { get; set; }
        public int UnPaidCount { get; set; }
        public Decimal UnPaidAmount { get; set; }
        public int TotalCount { get; set; }
        public Decimal TotalAmount { get; set; }
        public DateTime DateCrt { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
