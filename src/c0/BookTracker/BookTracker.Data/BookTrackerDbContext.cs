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

            entity.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Author>().HasData(SeedData.Authors);
        modelBuilder.Entity<Book>().HasData(SeedData.Books);
    }
}
