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
        public string FileBilling { get; set; } // file billing (file download)
        public string FileName { get; set; } // File upload

        public string source { get; set; }

        public DateTime? tglProses { get; set; }
        public int bankid_receipt { get; set; }
        public int bankid { get; set; }
        public int id_billing_download { get; set; }
        public string deskripsi { get; set; }
        public string stageTable { get; set; }
    }
}
