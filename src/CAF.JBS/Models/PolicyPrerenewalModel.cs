using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("policy_prerenewal")]
    public class PolicyPrerenewalModel
    {
        [Key]
        [Required]
        public int policy_Id { get; set; }
        public DateTime history_date { get; set; }
        public Decimal premium_amount { get; set; }
    }
}
