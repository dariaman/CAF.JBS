using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class UploadResultIndexVM
    {
        [Key]
        [Required]
        public int id { get; set; }

        public string trancode { get; set; }
        public string FileName { get; set; }
        public DateTime? tglProses { get; set; }
        public int billCountDwd { get; set; }
        public string deskripsi { get; set; }
    }
}
