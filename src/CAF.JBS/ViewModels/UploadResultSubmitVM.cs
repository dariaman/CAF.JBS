using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class UploadResultSubmitVM
    {
        [Required]
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "File belum tersedia ...")]
        [Display(Name = "File Result")]
        [DataType(DataType.Upload)]
        public IFormFile FileName { get; set; }

        [Required(ErrorMessage = "Tanggal Proses harus diisi ...")]
        public DateTime tglProses { get; set; }

        public string deskripsi { get; set; }
        
    }
}
