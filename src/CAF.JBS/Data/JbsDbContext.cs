﻿using System;
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
            builder.Entity<BillingSumMonthly>().HasKey(T => new { T.TranCode, T.Periode });
            builder.Entity<cctypeModel>().ToTable("cctype");
            builder.Entity<BankModel>().ToTable("bank");
            builder.Entity<BillingModel>().ToTable("billing");
            builder.Entity<prefixcardModel>().ToTable("prefixcard");
            builder.Entity<BillingHoldModel>().ToTable("billinghold");
            builder.Entity<PolicyBillingModel>().ToTable("policy_billing");
            builder.Entity<CustomerInfo>().ToTable("customer_info");
            builder.Entity<PolicyAcModel>().ToTable("policy_ac");
            builder.Entity<PolicyCCModel>().ToTable("policy_cc");
            builder.Entity<PolicyVa>().ToTable("policy_va");
            builder.Entity<PolicyLastTrans>().ToTable("policy_last_trans");
            builder.Entity<BillingSummary>().ToTable("billing_download_summary");
            builder.Entity<Product>().ToTable("product");
            builder.Entity<StagingUpload>().ToTable("stagingupload");
            builder.Entity<BillingSumMonthly>().ToTable("billing_sum_monthly");
            builder.Entity<TrancodeDashboard>().ToTable("trancode_dashboard");
            builder.Entity<Quote>().ToTable("quote");
            builder.Entity<QuoteBillingModel>().ToTable("quote_billing");
            builder.Entity<BillingOtherModel>().ToTable("billing_others");
            builder.Entity<FileNextProcessModel>().ToTable("FileNextProcess");
            builder.Entity<PolicyPrerenewalModel>().ToTable("policy_prerenewal");
            builder.Entity<UploadSumModel>().ToTable("upload_sum");
            builder.Entity<GroupRejectMappingModel>().ToTable("GroupRejectMapping");
            builder.Entity<ReasonMapingGroupModel>().ToTable("reason_maping_group");
            
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
        public DbSet<UploadSumModel> UploadSumModel { get; set; }
        public DbSet<GroupRejectMappingModel> GroupRejectMappingModel { get; set; }
        public DbSet<ReasonMapingGroupModel> ReasonMapingGroupModel { get; set; }
        
    }
}
