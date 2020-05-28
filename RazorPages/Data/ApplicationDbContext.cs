using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VMenu.Models;

namespace VMenu.Data
{
    public class ApplicationDbContext : IdentityDbContext<VmUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {
            var t = 1;
        }
        public DbSet<Restaurant> Restaurants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Restaurant>().ToTable("Restaurants");

            base.OnModelCreating(builder);
        }
    }
}
