using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("FileNextProcess")]
    public class FileNextProcessModel
    {
        [Required]
        [Key]
        public int id { get; set; }
        public string trancode { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public DateTime? tglProses { get; set; }
        public int bankid_receipt { get; set; }
        public string deskripsi { get; set; }
    }
}
