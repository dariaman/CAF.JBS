﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAF.JBS.Models
{
    [Table("policy_billing")]
    public class PolicyBillingModel
    {
        [Key]
        public int policy_Id { get; set; }
        public string policy_no { get; set; }
        public string payment_method { get; set; }
        public DateTime commence_dt { get; set; }
        public DateTime due_dt { get; set; }
        public int premium_mode { get; set; }
        public int product_id { get; set; }
        public int holder_id { get; set; }
        public decimal regular_premium { get; set; }
        public decimal cashless_fee_amount { get; set; }
        public int cycleDate { get; set; }
        public string CylceDateNotes { get; set; }

        public string Policy_status { get; set; }
        public DateTime? Policy_status_dateupdate { get; set; }
        public Boolean IsHoldBilling { get; set; }
        public string UserCrt { get; set; }
        public DateTime? DateCrt { get; set; }
        public string UserUpdate { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
