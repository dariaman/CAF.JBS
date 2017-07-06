using System;
using System.ComponentModel.DataAnnotations;

namespace CAF.JBS.ViewModels
{
    public class PolicyCycleDateVM
    {
        [Required]
        [Key]
        public int policy_Id { get; set; }
        public int cycleDate { get; set; }

        public string policy_no { get; set; }

        public string payment_method { get; set; }
        public DateTime commence_dt { get; set; }
        public int? premium_mode { get; set; }
        public string product_Name { get; set; }
        public string HolderName { get; set; }
        public string Status { get; set; }
        public decimal regular_premium { get; set; }
    }
}
