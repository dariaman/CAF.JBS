using System;

namespace CAF.JBS.ViewModels
{
    public class PolicyBillingViewModel
    {
        public string policy_Id { get; set; }
        public string policy_no { get; set; }
        public string payment_method { get; set; }
        public DateTime commence_dt { get; set; }
        public DateTime due_dt { get; set; }
        public string premium_mode { get; set; }
        public string product_description { get; set; }
        public string CustomerName { get; set; }
        public decimal regular_premium { get; set; }
        public decimal cashless_fee_amount { get; set; }
        public string Policy_status { get; set; }
        public bool IsHoldBilling { get; set; }
        public string cycleDate { get; set; }
        public string CylceDateNotes { get; set; }
        public DateTime? Policy_status_dateupdate { get; set; }
        public DateTime? DateCrt { get; set; }
        public Boolean IsWatchList { get; set; }
        public Boolean IsRenewal { get; set; }
        public string worksite_org_name { get; set; }
    }
}
