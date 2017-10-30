using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class RerateVM
    {
        public int policy_Id { get; set; }
        public String policy_No { get; set; }
        public DateTime history_date { get; set; }
        public Decimal premium_amount { get; set; }
    }
}
