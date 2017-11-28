using System;
using System.ComponentModel.DataAnnotations;

namespace CAF.JBS.ViewModels
{
    public class PolicyCycleDateVM
    {
        [Required]
        [Key]
        public int policy_Id { get; set; }
        public string policy_no { get; set; }

        [Required(ErrorMessage = "cycleDate tidak boleh kosong")]
        [Range(0, 31, ErrorMessage = "cycleDate harus diantara 1 - 31 !")]
        public int cycleDate { get; set; }
        public string CylceDateNotes { get; set; }
        public string payment_method { get; set; }
        public DateTime commence_dt { get; set; }
        public int? premium_mode { get; set; }
        public string product_Name { get; set; }
        public string HolderName { get; set; }
        public string Status { get; set; }
        public decimal regular_premium { get; set; }

        public string acc_no { get; set; }
        public string acc_name { get; set; }
        public string BankName { get; set; }
        public Boolean IsSkdr { get; set; }
    }
}
