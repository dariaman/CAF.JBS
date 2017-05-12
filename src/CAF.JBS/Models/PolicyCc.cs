using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("policy_cc")]
    public class PolicyCc
    {
        [Key]
        public int PolicyId { get; set; }
        public int cc_no { get; set; }
        public string cc_name { get; set; }
        public int bank_id { get; set; }
        public string bank_branch { get; set; }
        public string cc_expiry { get; set; }
        public string cc_address { get; set; }
        public string cc_telephone { get; set; }

        public string UserCrt { get; set; }
        public DateTime DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime DateUpdate { get; set; }
    }
}
