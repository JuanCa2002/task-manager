using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models.Entities;

namespace TaskManager
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskEntity>()
                .Property(t => t.Title)
                .HasMaxLength(250)
                .IsRequired();
        }

        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<StepEntity> Steps { get; set; }
        public DbSet<AttachedFileEntitty> AttachedFiles { get; set; }
    }
}
