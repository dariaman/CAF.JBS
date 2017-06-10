using CAF.JBS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class ReportViewModel
    {

        [DataType(DataType.Date, ErrorMessage = "Invalid Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime tgl { get; set; }

        public string thn { get; set; }
        public string bln { get; set; }

        public List<SelectListItem> blnList { get; set; }
        public IEnumerable<SelectListItem> thnList { get; set; }
    }
}
