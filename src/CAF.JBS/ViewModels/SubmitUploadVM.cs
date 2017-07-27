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

        public int CountDownload { get; set; }
        public int CountUpload { get; set; }

        public decimal SumDownload { get; set; }
        public decimal SumUpload { get; set; }

        public int CountApprove { get; set; }
        public decimal SumApprove { get; set; }

        public int CountReject { get; set; }
        public decimal SumReject { get; set; }

        public int CountKonflik { get; set; }
        public decimal SumFileKonflik { get; set; }
        public decimal SumBillKonflik { get; set; }
    }
}
