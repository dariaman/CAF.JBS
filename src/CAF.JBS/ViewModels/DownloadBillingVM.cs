using CAF.JBS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class DownloadBillingVM
    {
        public int id { get; set; }
        public string bank_collector { get; set; }
        public string file_download { get; set; }
        public string judul { get; set; }
        public string row_span { get; set; }
        public string group_name { get; set; }
        public string group_code { get; set; }

        public Decimal total_amount_billing { get; set; }
        public Decimal recurring_amount_billing { get; set; }
        public Decimal other_amount_billing { get; set; }
        public Decimal quote_amount_billing { get; set; }

        public int total_count_billing { get; set; }
        public int recurring_count_billing { get; set; }
        public int other_count_billing { get; set; }
        public int quote_count_billing { get; set; }

        public string Total { get; set; }
        public bool BcaCC { get; set; }
        public bool MandiriCC { get; set; }
        public bool MegaCC { get; set; }
        public bool BniCC { get; set; }
        public bool CimbCC { get; set; }

        public bool BcaAC { get; set; }
        public bool MandiriAC { get; set; }
    }
}
