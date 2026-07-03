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
        new() { Id = 1, Title = "The Pragmatic Programmer", AuthorId = 1, Isbn = "9780201616224", TotalPages = 352, Genre = "Software", CreatedAt = Seeded, Description = "A hands-on guide to the craft of software development. Through dozens of practical tips, it covers writing flexible and maintainable code, avoiding duplication with the DRY principle, using tracer bullets and prototypes, automating with the right tools, and taking pragmatic responsibility for your work. A career-spanning classic for developers who want to think about programming as a craft." },
        new() { Id = 2, Title = "Clean Code", AuthorId = 2, Isbn = "9780132350884", TotalPages = 464, Genre = "Software", CreatedAt = Seeded, Description = "A manual of software craftsmanship focused on writing code that humans can read. It argues that clean code is a matter of discipline: meaningful names, small functions that do one thing, clear comments only when needed, and rigorous handling of errors and boundaries. Includes detailed case studies that refactor messy code into clean code step by step." },
        new() { Id = 3, Title = "Domain-Driven Design", AuthorId = 3, Isbn = "9780321125217", TotalPages = 560, Genre = "Software", CreatedAt = Seeded, Description = "The foundational text on tackling complexity in software by modeling the business domain. It introduces a shared ubiquitous language between developers and domain experts, and building blocks such as entities, value objects, aggregates, repositories, and bounded contexts. A deep, strategic look at designing software that reflects how a business actually works." },
        new() { Id = 4, Title = "Dune", AuthorId = 4, Isbn = "9780441013593", TotalPages = 412, Genre = "Science Fiction", CreatedAt = Seeded, Description = "Set on the desert planet Arrakis, the only source of the spice melange, this epic follows young Paul Atreides as his family is betrayed and he is drawn into a struggle for control of the planet and its people. A sweeping story of politics, religion, ecology, and prophecy that became one of the most influential science fiction novels ever written." },
        new() { Id = 5, Title = "The Name of the Wind", AuthorId = 5, Isbn = "9780756404741", TotalPages = 662, Genre = "Fantasy", CreatedAt = Seeded, Description = "The first day in the recounted life of Kvothe: a gifted young man who grows up among traveling performers, survives tragedy and poverty, and talks his way into a legendary university of magic. Framed as a memoir told over three days, it is a lyrical coming-of-age fantasy about music, magic, love, and the gap between a legend and the truth behind it." },
    ];

    public static readonly Review[] Reviews =
    [
        new() { Id = 1, BookId = 1, Reviewer = "Dana", Rating = 5, Body = "Timeless advice; the tips still hold up.", CreatedOn = Seeded },
        new() { Id = 2, BookId = 1, Reviewer = "Lee", Rating = 4, Body = "A few examples feel dated but the principles don't.", CreatedOn = Seeded.AddDays(10) },
        new() { Id = 3, BookId = 2, Reviewer = "Sam", Rating = 5, Body = "Changed how I name things and size functions.", CreatedOn = Seeded.AddDays(3) },
        new() { Id = 4, BookId = 4, Reviewer = "Riley", Rating = 5, Body = "Sweeping world-building — a sci-fi landmark.", CreatedOn = Seeded.AddDays(7) },
        new() { Id = 5, BookId = 5, Reviewer = "Morgan", Rating = 4, Body = "Gorgeous prose; a slow but rewarding burn.", CreatedOn = Seeded.AddDays(1) },
    ];

    public static readonly ReadingProgress[] ReadingProgress =
    [
        // One book mid-read so GET /api/books/1/reading-progress returns data out of the box.
        new() { Id = 1, BookId = 1, CurrentPage = 100, Status = ReadingStatus.Reading, StartedOn = new DateOnly(2025, 1, 5) },
    ];
}
