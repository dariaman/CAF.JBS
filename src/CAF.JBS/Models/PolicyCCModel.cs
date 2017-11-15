using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("policy_cc")]
    public class PolicyCCModel
    {
        [Key]
        public int PolicyId { get; set; }
        public string cc_no { get; set; }
        public string cc_name { get; set; }
        public int bank_id { get; set; }
        public string cc_expiry { get; set; }
        public string cc_address { get; set; }
        public string cc_telephone { get; set; }
        
    }
}
