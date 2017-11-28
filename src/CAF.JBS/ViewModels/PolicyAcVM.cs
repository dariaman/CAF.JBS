using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class PolicyAcVM
    {
        public string PolicyId { get; set; }
        public string policy_no { get; set; }
        public string acc_no { get; set; }
        public string acc_name { get; set; }
        public string bank_code { get; set; }
        public string cycleDate { get; set; }
        public string cycleDateNote { get; set; }

        public Boolean IsSKDR { get; set; }
        public DateTime? DateCrt { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
