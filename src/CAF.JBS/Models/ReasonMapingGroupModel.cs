using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    public class ReasonMapingGroupModel
    {
        [Required]
        [Key]
        public int id { get; set; }
        public int? bank_id { get; set; }
        public string RejectCode { get; set; }
        public string RejectReason { get; set; }
        public int? GroupRejectMappingID { get; set; }

        public string user_crt { get; set; }
        public string user_update { get; set; }
        public string note { get; set; }

        public DateTime? DateCrt { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
