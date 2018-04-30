using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("policy_note")]
    public class policyNoteModel
    {
        [Required]
        [Key]
        public int policy_note_id { get; set; }
        public int policy_id { get; set; }
        public DateTime date_tran { get; set; }
        public string message { get; set; }
        public int staff_id { get; set; }
    }
}
