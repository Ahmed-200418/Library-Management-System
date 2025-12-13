namespace LibraryManagementSystem.Domain.Models

open System
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type User = {
    [<Key>]
    Id: int
    [<Required; MaxLength(200)>]
    Email: string
    [<Required>]
    PasswordHash: string
    Role: string
    CreatedAt: DateTime
}
