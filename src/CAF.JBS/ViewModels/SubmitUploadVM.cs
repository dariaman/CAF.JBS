using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class SubmitUploadVM
    {
        [Required(ErrorMessage = "Sesion telah habis, silahkan upload ulang file")]
        [Display(Name = "trancode")]
        public string trancode { get; set; }

        public IEnumerable<StagingUploadVM> StagingUploadVM { get; set; }
    }
}
