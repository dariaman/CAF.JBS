using CAF.JBS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class BillingSumMonthlyVM
    {
        public int TranCode { get; set; }
        public int PaidCount { get; set; }
        public Decimal PaidAmount { get; set; }
        public int UnPaidCount { get; set; }
        public Decimal UnPaidAmount { get; set; }
        public int CancelCount { get; set; }
        public Decimal CancelAmount { get; set; }
        public int TotalCount { get; set; }
        public Decimal TotalAmount { get; set; }
        public DateTime? DateUpdate { get; set; }

        public string DashName { get; set; }
    }
}
