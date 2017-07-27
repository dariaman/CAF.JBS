using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.ViewModels
{
    public class Quote
    {
        public int quote_id { get; set; }
        public string quote_status { get; set; }
        public DateTime quote_submitted_dt { get; set; }
    }
}
