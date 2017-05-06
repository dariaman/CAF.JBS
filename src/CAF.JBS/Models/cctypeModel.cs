using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("cctype")]
    public class cctypeModel
    {

        [Required]
        [Key]
        public int Id { get; set; }
        [Required]
        public string TypeCard { get; set; }
        //public List<SelectListItem> Banks { set; get; }
    }
}
