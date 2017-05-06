using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    public class BankModel
    {
        [Required]
        [Key]
        public int bank_id { get; set; }

        [Required]
        public string bank_code { get; set; }
    }
}
