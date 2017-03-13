using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    public class CardIssuerBankModel
    {
        [Required]
        [Key]
        public int card_issuer_bank_id { get; set; }
        public string bank_name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        [StringLength(6)]
        [RegularExpression("([0-9])")]
        public string Prefix { get; set; }
        public string Description { get; set; }

        [Required]
        public int  acquirer_bank_id { get; set; }        
    }
}
