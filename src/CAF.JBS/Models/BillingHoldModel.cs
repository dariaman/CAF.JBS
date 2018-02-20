using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("billinghold")]
    public class BillingHoldModel
    {
        [Required(ErrorMessage ="PolicyNo harus diisi")]
        [Key]
        [Display(Name = "PolicyNo")]
        public int policy_Id { get; set; }
        [Required(ErrorMessage ="Batas Tgl Hold Billing harus diisi")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
        public string Description { get; set; }

        public string UserCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
