﻿using System;
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
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<systemEmailQueueModel>().Property(x => x.email_body).HasColumnType("text");
            builder.Entity<systemEmailQueueModel>().ToTable("system_email_queue");
            builder.Entity<policyNoteModel>().ToTable("policy_note");
            builder.Entity<receiptOtherModel>().ToTable("receipt_other");
            builder.Entity<receiptModel>().ToTable("receipt");
        }

        public DbSet<systemEmailQueueModel> systemEmailQueueModel { get; set; }
        public DbSet<policyNoteModel> policyNoteModel { get; set; }
        public DbSet<receiptOtherModel> receiptOtherModel { get; set; }
        public DbSet<receiptModel> receiptModel { get; set; }
    }
}
