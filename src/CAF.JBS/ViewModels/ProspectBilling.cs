using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class ProspectBilling
    {
        public int quote_id { get; set; }
        public string prospect_convert_flag { get; set; }
        public string prospect_appr_code { get; set; }
        public DateTime updated_dt { get; set; }
        public int acquirer_bank_id { get; set; }
    }
}
