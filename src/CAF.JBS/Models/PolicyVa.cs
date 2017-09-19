using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("policy_va")]
    public class PolicyVa
    {
        [Key]
        public int PolicyId { get; set; }
        public int VANo { get; set; }
        public string VAName { get; set; }
        public int bank_id { get; set; }

        public string UserCrt { get; set; }
        public DateTime? DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
