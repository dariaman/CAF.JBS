using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    public class UploadSumModel
    {
        [Required]
        [Key]
        public int id { get; set; }
        public string deskripsi { get; set; }
        public int total_upload { get; set; }
        public int count_approve { get; set; }
        public int count_reject { get; set; }
    }
}
