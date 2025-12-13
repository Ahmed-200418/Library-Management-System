namespace LibraryManagementSystem.Domain.DTOs

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type RegisterDto = {
    Email: string
    Password: string
    RememberMe: bool
}
