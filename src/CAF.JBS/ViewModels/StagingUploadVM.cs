using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class StagingUploadVM
    {
        // data download
        public string polistran { get; set; }
        public string BillCode { get; set; }
        public Nullable<DateTime> tgl { get; set; }
        public Decimal amount { get; set; }
        public Boolean IsSuccess { get; set; }
        public int? PolicyNo { get; set; }
        public string Billid { get; set; }
        public int? ReqSeq { get; set; }
        public Decimal billAmount { get; set; }
    }
}
