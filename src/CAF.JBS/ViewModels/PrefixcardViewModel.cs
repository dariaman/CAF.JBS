using CAF.JBS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class PrefixcardViewModel
    {
        public int Prefix { get; set; }
        public string TypeCard { get; set; }
        public string BankName { get; set; }
        public string Description { get; set; }

        public List<cctypeModel> CCtype { get; set; }
        public List<BankModel> banks { get; set; }
    }
}
