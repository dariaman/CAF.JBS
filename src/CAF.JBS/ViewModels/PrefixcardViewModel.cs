using CAF.JBS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class PrefixcardViewModel
    {
        [Key]
        [Display(Name = "Bin Number")]
        [Required(ErrorMessage = "Bin Number harus diisi")]
        [MinLength(6,ErrorMessage ="Minimal 6 karakter")]
        [StringLength(8,ErrorMessage ="Maksimal 8 karakter")]
        public int Prefix { get; set; }
        [Required(ErrorMessage = "Bank Penerbit harus diisi")]
        public int bank_id { get; set; }
        public int Type { get; set; }

        [MaxLength(255,ErrorMessage ="Maksimal karakter 255")]
        public string Description { get; set; }
        
        public IEnumerable<SelectListItem> CCtypes { get; set; }
        public IEnumerable<SelectListItem> banks { get; set; }
        public string BankName { get; set; }
        public string TypeCard { get; set; }

    }
}
