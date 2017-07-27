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

        public int TCountDownload { get; set; }
        public int BCountDw { get; set; }
        public int ACountDw { get; set; }
        public int QCountDw { get; set; }
        public decimal TSumDownload { get; set; }
        public decimal BSumDw { get; set; }
        public decimal ASumDw { get; set; }
        public decimal QSumDw { get; set; }

        public int CountUpload { get; set; }
        public int BCountUp { get; set; }
        public int ACountUp { get; set; }
        public int QCountUp { get; set; }
        public decimal SumUpload { get; set; }
        public decimal BSumUp { get; set; }
        public decimal ASumUp { get; set; }
        public decimal QSumUp { get; set; }

        public int CountApprove { get; set; }
        public int BCountUpAp { get; set; }
        public int ACountUpAp { get; set; }
        public int QCountUpAp { get; set; }
        public decimal SumApprove { get; set; }
        public decimal BSumUpAp { get; set; }
        public decimal ASumUpAp { get; set; }
        public decimal QSumUpAp { get; set; }

        public int CountReject { get; set; }
        public int BCountUpRj { get; set; }
        public int ACountUpRj { get; set; }
        public int QCountUpRj { get; set; }
        public decimal SumReject { get; set; }
        public decimal BSumUpRj { get; set; }
        public decimal ASumUpRj { get; set; }
        public decimal QSumUpRj { get; set; }

        public int CountKonflik { get; set; }
        public decimal SumFileKonflik { get; set; } // Amount
        public decimal SumBillKonflik { get; set; } // Amount
    }
}
