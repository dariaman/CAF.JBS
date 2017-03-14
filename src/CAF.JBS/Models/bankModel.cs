using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    public class bankModel
    {
        [Required]
        [Key]
        //[Column("bank_id")]
        public int bank_id { get; set; }
        public string bank_code { get; set; }
        public int bank_acquiring_flag { get; set; }
    }
}
