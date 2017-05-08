
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.ViewModels
{
    public class BillingHoldViewModel : IValidatableObject
    {
        
        [Key]
        [Required(ErrorMessage = "PolicyNo harus diisi")]
        public int policy_Id { get; set; }

        
        [Display(Name = "PolicyNo")]
        public string policy_No { get; set; }

        [Required(ErrorMessage = "Batas Tgl Hold Billing harus diisi")]
        [Display(Name = "Hold Date")]
        [DataType(DataType.Date, ErrorMessage = "Invalid Date")]

        public DateTime ReleaseDate { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            if (ReleaseDate < DateTime.Today)
            {
                ValidationResult mss = new ValidationResult("Hold Date harus minimal tanggal sekarang");
                res.Add(mss);

            }
            return res;
        }
    }
}
