using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("policy_last_trans")]
    public class PolicyLastTrans
    {
        [Key]
        public int PolicyId { get; set; }
        public int BillingID { get; set; }
        public int recurring_seq { get; set; }
        public DateTime due_dt_pre { get; set; }
        public string source { get; set; }
        public int receipt_id { get; set; }
        public DateTime receipt_date { get; set; }

        public string UserCrt { get; set; }
        public DateTime? DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
