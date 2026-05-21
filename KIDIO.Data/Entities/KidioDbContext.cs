using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Data.Entities
{
    public class KidioDbContext : DbContext
    {
        public KidioDbContext(DbContextOptions<KidioDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Child> Children => Set<Child>();
        public DbSet<Topic> Topics => Set<Topic>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
        public DbSet<Achievement> Achievements => Set<Achievement>();
        public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
        public DbSet<PronunciationLog> PronunciationLogs => Set<PronunciationLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global filter: soft delete
            modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Child>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Lesson>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Topic>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Vocabulary>().HasQueryFilter(x => !x.IsDeleted);

            // User
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).HasMaxLength(256);
                e.Property(u => u.DisplayName).HasMaxLength(100);
            });

            // Child -> User (Parent)
            modelBuilder.Entity<Child>(e =>
            {
                e.HasOne(c => c.Parent)
                 .WithMany(u => u.Children)
                 .HasForeignKey(c => c.ParentId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(c => c.Name).HasMaxLength(100);
            });

            // LessonProgress: unique (ChildId, LessonId)
            modelBuilder.Entity<LessonProgress>(e =>
            {
                e.HasIndex(p => new { p.ChildId, p.LessonId }).IsUnique();
                e.HasOne(p => p.Child).WithMany(c => c.Progresses)
                 .HasForeignKey(p => p.ChildId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(p => p.Lesson).WithMany(l => l.Progresses)
                 .HasForeignKey(p => p.LessonId).OnDelete(DeleteBehavior.Restrict);
            });

            // Topic
            modelBuilder.Entity<Topic>(e =>
            {
                e.HasIndex(t => t.OrderIndex).IsUnique();
            });

            // Lesson -> Topic
            modelBuilder.Entity<Lesson>(e =>
            {
                e.HasOne(l => l.Topic).WithMany(t => t.Lessons)
                 .HasForeignKey(l => l.TopicId).OnDelete(DeleteBehavior.Restrict);
                e.Property(l => l.Title).HasMaxLength(200);
            });

            // PronunciationLog
            modelBuilder.Entity<PronunciationLog>(e =>
            {
                e.HasOne(p => p.Child).WithMany(c => c.PronunciationLogs)
                 .HasForeignKey(p => p.ChildId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(p => p.Lesson).WithMany()
                 .HasForeignKey(p => p.LessonId).OnDelete(DeleteBehavior.SetNull);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            // Auto-set UpdatedAt
            foreach (var entry in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified))
            {
                if (entry.Entity is KIDIO.Common.BaseEntity entity)
                    entity.UpdatedAt = DateTime.UtcNow;
            }
            return base.SaveChangesAsync(ct);
        }
    
    }
}
