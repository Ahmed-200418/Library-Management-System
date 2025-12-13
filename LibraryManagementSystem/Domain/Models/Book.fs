namespace LibraryManagementSystem.Domain.Models

open System
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type Book = {
    [<Key>]
    Id: int
    [<Required; MaxLength(200)>]
    Title: string
    [<Required; MaxLength(200)>]
    Author: string
    Description: string
    ImagePath: string
    IsBorrowed: bool
    CreatedAt: DateTime
}
