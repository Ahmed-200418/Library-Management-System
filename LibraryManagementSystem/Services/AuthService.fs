namespace LibraryManagementSystem.Services

open System
open System.Text.RegularExpressions
open System.Threading.Tasks
open Microsoft.EntityFrameworkCore
open BCrypt.Net
open LibraryManagementSystem.Data
open LibraryManagementSystem.Domain.Models
open LibraryManagementSystem.Domain.DTOs

type AuthService(context: LibraryDbContext) =
    
    // Simple Email Regex: anything@anything.anything
    let emailRegex = Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")

    member this.RegisterAsync (dto: RegisterDto) =
        async {
            // --- VALIDATION LOGIC ---
            if String.IsNullOrWhiteSpace(dto.Email) then
                return Error "Email is required"
            else if String.IsNullOrWhiteSpace(dto.Password) then
                return Error "Password is required"
            else
                // 1. Trim first (Safe to do here because we passed the NullOrWhiteSpace check)
                let trimmedEmail = dto.Email.Trim()

                // 2. Validate format using the TRIMMED email
                if not (emailRegex.IsMatch(trimmedEmail)) then
                    return Error "Invalid email format"
                else if dto.Password.Length < 6 then 
                     return Error "Password must be at least 6 characters"
                else
                    // 3. Normalize: Lowercase for case-insensitive check and storage
                    let normalizedEmail = trimmedEmail.ToLower()

                    let! existing = context.Users.FirstOrDefaultAsync(fun u -> u.Email = normalizedEmail) |> Async.AwaitTask
                    match Option.ofObj existing with
                    | Some _ -> return Error "User already exists"
                    | None ->
                        let user = {
                            Id = 0
                            Email = normalizedEmail // Save normalized
                            PasswordHash = BCrypt.HashPassword(dto.Password)
                            Role = "User"
                            CreatedAt = DateTime.UtcNow
                        }
                        context.Users.Add(user) |> ignore
                        let! _ = context.SaveChangesAsync() |> Async.AwaitTask
                        return Ok user
        } |> Async.StartAsTask

    member this.ValidateUserAsync (dto: LoginDto) =
        async {
            try
                // Input validation
                if String.IsNullOrWhiteSpace(dto.Email) then
                    return Error "Email is required"
                else if String.IsNullOrWhiteSpace(dto.Password) then
                    return Error "Password is required"
                else
                    // Normalize input to match stored email
                    let normalizedEmail = dto.Email.Trim().ToLower()

                    let! user = context.Users.FirstOrDefaultAsync(fun u -> u.Email = normalizedEmail) |> Async.AwaitTask
                    match Option.ofObj user with
                    | None -> return Error "Invalid credentials"
                    | Some u ->
                        if BCrypt.Verify(dto.Password, u.PasswordHash) then
                            return Ok u
                        else
                            return Error "Invalid credentials"
            with ex ->
                return Error $"An error occurred during authentication: {ex.Message}"
        } |> Async.StartAsTask