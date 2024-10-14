using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResondLawnSevices.Models;

namespace ResondLawnSevices.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {

        }


        public DbSet<Machine> Machines { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<Conflicts> Conflicts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Conflicts>()
                .HasKey(c => c.Id); // Primary key

           
            modelBuilder.Entity<Conflicts>()
                .HasOne(c => c.User)
                .WithMany(u => u.Conflicts)
                .HasForeignKey(c => c.UserId); // Foreign key in Conflict table

        }

        //protected Conflict void OnModelCreating(ModelBuilder modelBuilder)
        //{


        //    modelBuilder.Entity<Booking>()
        //        .HasKey(b => b.Id);

        //    modelBuilder.Entity<Booking>()
        //        .HasOne(b => b.User)
        //        .WithMany()
        //        .HasForeignKey(b => b.UserId);

        //    modelBuilder.Entity<Booking>()
        //        .HasOne(b => b.Machine)
        //        .WithMany()
        //        .HasForeignKey(b => b.MachineId);
        //}
    }
}
