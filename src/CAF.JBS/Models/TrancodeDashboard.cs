using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("trancode_dashboard")]
    public class TrancodeDashboard
    {
        [Key]
        [Required]
        public int TranCode { get; set; }
        public string DashName { get; set; }
        public string Description { get; set; }
    }
}
