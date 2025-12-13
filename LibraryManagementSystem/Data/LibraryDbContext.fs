namespace LibraryManagementSystem.Data

open Microsoft.EntityFrameworkCore
open LibraryManagementSystem.Domain.Models

type LibraryDbContext(options: DbContextOptions<LibraryDbContext>) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable users : DbSet<User>
    [<DefaultValue>]
    val mutable books : DbSet<Book>
    [<DefaultValue>]
    val mutable borrowedBooks : DbSet<BorrowedBook>

    member this.Users with get() = this.users and set v = this.users <- v
    member this.Books with get() = this.books and set v = this.books <- v
    member this.BorrowedBooks with get() = this.borrowedBooks and set v = this.borrowedBooks <- v

    override this.OnModelCreating(modelBuilder: ModelBuilder) =
        // Ensure Email is unique
        modelBuilder.Entity<User>()
            .HasIndex(fun u -> u.Email :> obj)
            .IsUnique() |> ignore