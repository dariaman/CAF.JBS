using System;
using System.ComponentModel.DataAnnotations;

namespace CAF.JBS.ViewModels
{
    public class ReasonMapingGroupVM
    {
        public string id { get; set; }
        public string bank { get; set; }
        public string RejectCode { get; set; }
        public string RejectReason { get; set; }
        public string GroupReject { get; set; }

        public string note { get; set; }

    }
}
