using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("GroupRejectMapping")]
    public class GroupRejectMappingModel
    {
        [Required]
        [Key]
        public int id { get;  }
        public string GroupRejectReason { get; set; }
        public DateTime? DateCrt { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
