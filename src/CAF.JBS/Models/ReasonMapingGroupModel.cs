using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("ReasonMapingGroup")]
    public class ReasonMapingGroupModel
    {
        [Required]
        [Key]
        public int id { get; }
        public string ReajectReason { get; set; }
        public int GroupRejectMappingID { get; set; }
        public DateTime? DateCrt { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
