using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class QuoteBillingVM
    {
        public int quote_id { get; set; }
        public int product_id { get; set; }
        public int policy_id { get; set; }
        public string ref_no { get; set; }
        public string CustomerName { get; set; }
        public Decimal prospect_amount { get; set; }
        public Decimal? paper_print_fee { get; set; }
        public Decimal? cashless_fee { get; set; }
        public Decimal TotalAmount { get; set; }
        public DateTime? cancel_date { get; set; }
        public DateTime? paid_dt { get; set; }

        public string status { get; set; }
        public DateTime? DateCrt { get; set; }
        public DateTime? LastUploadDate { get; set; }
    }
}
