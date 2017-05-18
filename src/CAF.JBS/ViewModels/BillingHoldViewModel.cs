
using CAF.JBS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CAF.JBS.ViewModels
{
    public class BillingHoldViewModel
    {
        [Key]
        public int policy_Id { get; set; }

        [Required(ErrorMessage = "PolicyNo harus diisi")]
        [Display(Name = "PolicyNo")]
        public string policy_No { get; set; }

        [Required(ErrorMessage = "Hold Date Billing harus diisi")]
        [Display(Name = "Hold Date")]
        [DataType(DataType.Date, ErrorMessage = "Invalid Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReleaseDate { get; set; }
        public string Description { get; set; }
    }
}
