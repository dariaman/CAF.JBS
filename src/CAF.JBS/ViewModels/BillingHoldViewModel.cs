
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.ViewModels
{
    public class BillingHoldViewModel
    {
        [Required(ErrorMessage ="PolicyNo harus diisi")]
        [Key]
        [Display(Name = "PolicyNo")]
        public string policy_No { get; set; }
        [Required(ErrorMessage ="Batas Tgl Hold Billing harus diisi")]
        [DataType(DataType.Date)]
        [Display(Name = "Hold Date")]
        public DateTime ReleaseDate { get; set; }
        public int OldpolicyID { get; set; }
    }
}
