using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class UploadResultBillingVM
    {
        [Key]
        [Required]
        public string TranCode { get; set; }
        //[Required(ErrorMessage = "File harus diisi ...")]
        //public string FileName { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "File harus diisi ...")]
        [DataType(DataType.Upload)]
        //[FileExtensions(Extensions = "xls")]
        [Display(Name = "File Result")]
        public IFormFile FileBill { get; set; }

        //[DataType(DataType.Upload)]
        //HttpPostedFileBase ImageUpload { get; set; }
    }
}
