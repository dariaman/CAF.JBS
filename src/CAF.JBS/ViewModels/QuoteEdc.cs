using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class QuoteEdc
    {
        public int quote_id { get; set; }
        public int status_id { get; set; }
        public string appr_code { get; set; }
        public string reason { get; set; }
    }
}
