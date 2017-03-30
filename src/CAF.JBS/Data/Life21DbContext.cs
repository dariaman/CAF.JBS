using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Models;

namespace CAF.JBS.Data
{
    public class Life21DbContext : IdentityDbContext<ApplicationUser>
    {
        public Life21DbContext(DbContextOptions<Life21DbContext> options) : base(options)
        {        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<CardIssuerBankModel>().ToTable("card_issuer_bank");
            builder.Entity<BankModel>().ToTable("bank");
        }

        public DbSet<CardIssuerBankModel> CardIssuerBankModel { get; set; }
        public DbSet<BankModel> BankModel { get; set; }
    }
}
