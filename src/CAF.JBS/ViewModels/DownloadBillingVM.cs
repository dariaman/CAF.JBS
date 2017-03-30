using System;
using System.Collections.Generic;
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
    }
}
