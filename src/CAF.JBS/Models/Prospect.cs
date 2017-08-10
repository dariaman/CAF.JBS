using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("prospect")]
    public class Prospect
    {
        [Key]
        [Required]
        public int prospect_id { get; set; }
        public string prospect_name { get; set; }
        public string prospect_birth_place { get; set; }
        public DateTime prospect_dob { get; set; }
        public string prospect_gender { get; set; }
        public string prospect_mobile_phone { get; set; }
        public string prospect_email { get; set; }
        public int coverage_type_id { get; set; }
    }
}
