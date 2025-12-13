namespace LibraryManagementSystem.Domain.DTOs

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type LoginDto = {
    Email: string
    Password: string
    RememberMe: bool
}
