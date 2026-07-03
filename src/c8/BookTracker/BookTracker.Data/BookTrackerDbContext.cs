using BookTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Data;

public class BookTrackerDbContext : DbContext
{
    public BookTrackerDbContext(DbContextOptions<BookTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<ReadingProgress> ReadingProgress => Set<ReadingProgress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(300);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(500);
            entity.Property(b => b.Isbn).HasMaxLength(20);
            entity.Property(b => b.Genre).HasMaxLength(100);
            entity.Property(b => b.Description).HasMaxLength(2000);

            entity.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Reviewer).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Body).IsRequired().HasMaxLength(4000);

            entity.HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReadingProgress>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(p => p.BookId).IsUnique();

            entity.HasOne(p => p.Book)
                .WithMany()
                .HasForeignKey(p => p.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Author>().HasData(SeedData.Authors);
        modelBuilder.Entity<Book>().HasData(SeedData.Books);
        modelBuilder.Entity<Review>().HasData(SeedData.Reviews);
        modelBuilder.Entity<ReadingProgress>().HasData(SeedData.ReadingProgress);
    }
}
