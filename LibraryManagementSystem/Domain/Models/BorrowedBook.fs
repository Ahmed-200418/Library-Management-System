namespace LibraryManagementSystem.Domain.Models

open System
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type BorrowedBook = {
    [<Key>]
    Id: int
    [<Required>]
    UserId: int
    [<Required>]
    BookId: int
    [<Required>]
    BorrowDate: DateTime
    ReturnDate: Nullable<DateTime>
}
