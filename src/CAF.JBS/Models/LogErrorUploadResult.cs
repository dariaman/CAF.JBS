using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    public class LogErrorUploadResult
    {
        public LogErrorUploadResult()
        {
            DateCrt = DateTime.Now;
        }
        public string TranCode { get; set; }
        public int line { get; set; }
        public string FileName { get; set; }
        public string exceptionApp { get; set; }
        public DateTime DateCrt { get; set; }
    }
}
