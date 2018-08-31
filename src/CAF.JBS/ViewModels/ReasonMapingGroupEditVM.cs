using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CAF.JBS.ViewModels
{
    public class ReasonMapingGroupEditVM
    {
        public string id { get; set; }
        public string bank_id { get; set; }
        public string bank_name { get; set; }
        public string RejectCode { get; set; }
        public string RejectReason { get; set; }
        public string GroupRejectMappingID { get; set; }
        public string GroupReject_Description { get; set; }
        public string note { get; set; }

        public IEnumerable<SelectListItem> GroupReject { get; set; }
        public IEnumerable<SelectListItem> banks { get; set; }

    }
}
