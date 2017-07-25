using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("stagingupload")]
    public class StagingUpload
    {
        [Key]
        public int id { get; set; }
        public string polisNo { get; set; }
        public string BillCode { get; set; }
        public Nullable<DateTime> tgl { get; set; }
        public Decimal amount { get; set; }
        public Boolean IsSuccess { get; set; }
        public string ApprovalCode { get; set; }
        public string Description { get; set; }
        public string ACCno { get; set; }
        public string trancode { get; set; }
        public string filename { get; set; }
        public int? PolicyId { get; set; }
        public string Billid { get; set; }
    }
}
