using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Models
{
    [Table("customer_info")]
    public class CustomerInfo
    {
        [Key]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime dob { get; set; }
        public Boolean IsLaki { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string UserCrt { get; set; }
        public DateTime? DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
