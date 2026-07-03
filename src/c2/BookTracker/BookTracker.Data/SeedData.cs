using BookTracker.Core.Entities;

namespace BookTracker.Data;

internal static class SeedData
{
    // Fixed timestamps keep migrations deterministic.
    private static readonly DateTimeOffset Seeded = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static readonly Author[] Authors =
    [
        new() { Id = 1, Name = "Andrew Hunt, David Thomas" },
        new() { Id = 2, Name = "Robert C. Martin" },
        new() { Id = 3, Name = "Eric Evans" },
        new() { Id = 4, Name = "Frank Herbert" },
        new() { Id = 5, Name = "Patrick Rothfuss" },
    ];

    public static readonly Book[] Books =
    [
        new() { Id = 1, Title = "The Pragmatic Programmer", AuthorId = 1, Isbn = "9780201616224", TotalPages = 352, Genre = "Software", CreatedAt = Seeded },
        new() { Id = 2, Title = "Clean Code", AuthorId = 2, Isbn = "9780132350884", TotalPages = 464, Genre = "Software", CreatedAt = Seeded },
        new() { Id = 3, Title = "Domain-Driven Design", AuthorId = 3, Isbn = "9780321125217", TotalPages = 560, Genre = "Software", CreatedAt = Seeded },
        new() { Id = 4, Title = "Dune", AuthorId = 4, Isbn = "9780441013593", TotalPages = 412, Genre = "Science Fiction", CreatedAt = Seeded },
        new() { Id = 5, Title = "The Name of the Wind", AuthorId = 5, Isbn = "9780756404741", TotalPages = 662, Genre = "Fantasy", CreatedAt = Seeded },
    ];

    public static readonly Review[] Reviews =
    [
        new() { Id = 1, BookId = 1, Reviewer = "Dana", Rating = 5, Body = "Timeless advice; the tips still hold up.", CreatedOn = Seeded },
        new() { Id = 2, BookId = 1, Reviewer = "Lee", Rating = 4, Body = "A few examples feel dated but the principles don't.", CreatedOn = Seeded.AddDays(10) },
        new() { Id = 3, BookId = 2, Reviewer = "Sam", Rating = 5, Body = "Changed how I name things and size functions.", CreatedOn = Seeded.AddDays(3) },
        new() { Id = 4, BookId = 4, Reviewer = "Riley", Rating = 5, Body = "Sweeping world-building — a sci-fi landmark.", CreatedOn = Seeded.AddDays(7) },
        new() { Id = 5, BookId = 5, Reviewer = "Morgan", Rating = 4, Body = "Gorgeous prose; a slow but rewarding burn.", CreatedOn = Seeded.AddDays(1) },
    ];
}
