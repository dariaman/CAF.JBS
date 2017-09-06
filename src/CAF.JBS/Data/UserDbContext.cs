using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CAF.JBS.Models;

namespace CAF.JBS.Data
{
    public class UserDbContext : IdentityDbContext<ApplicationUser>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<IdentityUser>().ToTable("aspnetusers");
            builder.Entity<IdentityRole>().ToTable("aspnetroles");
            //builder.Entity<IdentityUserRole>().ToTable("Users", "dbo").Property(p => p.Id).HasColumnName("User_Id");

            //builder.Entity<Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole>().ToTable("AspNetUsers");
            //builder.Entity<IdentityUser>().ToTable("AspNetUsers");
            //builder.Entity<IdentityUser>().ToTable("AspNetUsers");
            //builder.Entity<IdentityUser>().ToTable("AspNetUsers");

            //builder.Entity<ApplicationUser>().ToTable("User");
            //builder.Entity<IdentityRole>().ToTable("Role");
            //builder.Entity<Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole>().ToTable("UserRole");
            //builder.Entity<Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim>().ToTable("UserClaim");
            //builder.Entity<Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin>().ToTable("UserLogin");
        }

    }
}
