using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class PolicyAcVM
    {
        [Key]
        public int PolicyId { get; set; }
        public string acc_no { get; set; }
        public string acc_name { get; set; }
        public int bank_id { get; set; }
        public string bank_branch { get; set; }

        public int? cycleDate { get; set; }
        public Boolean IsSKDR { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
