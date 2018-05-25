using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Models;

namespace CAF.JBS.Data
{
    public class JbsDbContext : IdentityDbContext<ApplicationUser>
    {
        public JbsDbContext(DbContextOptions<JbsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<cctypeModel> cctypeModel { get; set; }
        public DbSet<BankModel> BankModel { get; set; }
        public DbSet<BillingModel> BillingModel { get; set; }
        public DbSet<prefixcardModel> prefixcardModel { get; set; }
        public DbSet<BillingHoldModel> BillingHoldModel { get; set; }
        public DbSet<PolicyBillingModel> PolicyBillingModel { get; set; }
        public DbSet<CustomerInfo> CustomerInfo { get; set; }
        public DbSet<PolicyAcModel> PolicyAcModel { get; set; }
        public DbSet<PolicyCCModel> PolicyCCModel { get; set; }
        public DbSet<PolicyVa> PolicyVa { get; set; }
        public DbSet<PolicyLastTrans> PolicyLastTrans { get; set; }
        public DbSet<BillingSummary> BillingSummary { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<StagingUpload> StagingUpload { get; set; }
        public DbSet<BillingSumMonthly> BillingSumMonthly { get; set; }
        public DbSet<TrancodeDashboard> TrancodeDashboard { get; set; }
        public DbSet<Quote> Quote { get; set; }
        public DbSet<QuoteBillingModel> QuoteBillingModel { get; set; }
        public DbSet<BillingOtherModel> BillingOtherModel { get; set; }
        public DbSet<FileNextProcessModel> FileNextProcessModel { get; set; }
        public DbSet<PolicyPrerenewalModel> PolicyPrerenewalModel { get; set; }
        public DbSet<RejectReasonModel> RejectReasonModel { get; set; }
    }
}
