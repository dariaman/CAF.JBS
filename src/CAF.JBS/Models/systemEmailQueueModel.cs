using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("system_email_queue")]
    public class systemEmailQueueModel
    {
        [Required]
        [Key]
        public int email_queue_id { get; set; }
        public DateTime? email_sent_date { get; set; }
        public string email_to { get; set; }
        public string email_bcc { get; set; }
        public string email_subject { get; set; }

        //[Column(TypeName = "varchar")]
        //[StringLength(int.MaxValue)]
        public string email_body { get; set; }
        public DateTime email_created_dt { get; set; }
        public string email_type { get; set; }
        public string email_status { get; set; }
        
    }
}
