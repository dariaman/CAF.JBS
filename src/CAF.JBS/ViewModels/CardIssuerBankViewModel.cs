using CAF.JBS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class CardIssuerBankViewModel
    {
        [Required]
        [Key]
        public int card_issuer_bank_id { get; set; }

        [Required]
        [StringLength(6)]
        [RegularExpression("([0-9])")]
        public string Prefix { get; set; }
        public string TypeCard { get; set; }
        public string BankName { get; set; }
        public string Description { get; set; }
    }
}
