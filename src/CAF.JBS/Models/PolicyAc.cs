using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("policy_ac")]
    public class PolicyAc
    {
        [Key]
        public int PolicyId { get; set; }
        public int acc_no { get; set; }
        public string acc_name { get; set; }
        public int bank_id { get; set; }
        public string bank_branch { get; set; }

        public string UserCrt { get; set; }
        public DateTime DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime DateUpdate { get; set; }
    }
}
