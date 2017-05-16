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
        public string Total { get; set; }
        public bool BcaCC { get; set; }
        public bool MandiriCC { get; set; }
        public bool BniCC { get; set; }
        public bool MegaCC { get; set; }

        public bool BcaAC { get; set; }
        public bool MandiriAC { get; set; }

        public bool BcaRegularPremium { get; set; }

        public bool BCAOther { get; set; }

        public Decimal? BCAccTotal { get; set; }
        public Decimal? MandiriccTotal { get; set; }
        public Decimal? MegaOnUSccTotal { get; set; }
        public Decimal? MegaOfUsccTotal { get; set; }
        public Decimal? BNIccTotal { get; set; }
        public Decimal? BCAacTotal { get; set; }
        public Decimal? MandiriACTotal { get; set; }

        public List<BillingSummary> BillingSummary { get; set; }
    }
}
