using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class PolicyCcVM
    {
        public string PolicyId { get; set; }
        public string policy_no { get; set; }

        public string cc_no { get; set; }
        public string cc_name { get; set; }
        public string cc_expiry { get; set; }
        public string bank_code { get; set; }

        public DateTime? DateCrt { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
