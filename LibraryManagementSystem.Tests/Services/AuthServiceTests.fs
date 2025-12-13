namespace LibraryManagementSystem.Tests.Services

open System
open Xunit
open FsUnit.Xunit
open LibraryManagementSystem.Services
open LibraryManagementSystem.Domain.DTOs
open LibraryManagementSystem.Tests

type AuthServiceTests() =

    // --- RegisterAsync Tests ---

    [<Fact>]
    member this.``RegisterAsync should create user when inputs are valid``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Reg_Valid"
            let service = AuthService(context)
            let dto : RegisterDto = { Email = "valid@test.com"; Password = "password123"; RememberMe = false }
            
            let! result = service.RegisterAsync(dto)
            
            match result with
            | Ok user -> 
                user.Email |> should equal "valid@test.com"
                user.Role |> should equal "User" 
                user.PasswordHash |> should not' (equal "password123") 
                // Time check with parenthesis fix
                user.CreatedAt |> should be (lessThan (DateTime.UtcNow.AddSeconds(1.0)))
            | Error msg -> failwith $"Expected success but got: {msg}"
        }

    [<Fact>]
    member this.``RegisterAsync should fail when email is empty``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Reg_EmptyEmail"
            let service = AuthService(context)
            let dto = { Email = ""; Password = "password123"; RememberMe = false }
            
            let! result = service.RegisterAsync(dto)
            match result with | Error msg -> Assert.Contains("required", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``RegisterAsync should fail when password is empty``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Reg_PassEmpty"
            let service = AuthService(context)
            let dto = { Email = "valid@test.com"; Password = ""; RememberMe = false }
            
            let! result = service.RegisterAsync(dto)
            match result with | Error msg -> Assert.Contains("required", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``RegisterAsync should fail when password is too short``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Reg_ShortPass"
            let service = AuthService(context)
            let dto = { Email = "test@test.com"; Password = "123"; RememberMe = false }
            
            let! result = service.RegisterAsync(dto)
            match result with
            | Error _ -> () // Pass
            | Ok _ -> failwith "Should fail (password too short)" 
        }

    [<Fact>]
    member this.``RegisterAsync should fail when email format is invalid``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Reg_EmailFormat"
            let service = AuthService(context)
            let invalidEmails = [ "abc"; "user@"; "user.com"; "user@com"; "@domain.com" ]
            
            for email in invalidEmails do
                let dto = { Email = email; Password = "password123"; RememberMe = false }
                let! result = service.RegisterAsync(dto)
                match result with 
                | Error msg -> Assert.Contains("Invalid email", msg) 
                | Ok _ -> failwith $"Should fail for invalid email: {email}"
        }

    [<Fact>]
    member this.``RegisterAsync should trim email before saving``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Reg_Trim"
            let service = AuthService(context)
            let dto = { Email = "  spaced@test.com  "; Password = "password123"; RememberMe = false }
            
            let! result = service.RegisterAsync(dto)
            match result with | Ok user -> user.Email |> should equal "spaced@test.com" | Error e -> failwith e
        }

    [<Fact>]
    member this.``RegisterAsync should treat email as case-insensitive``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Reg_Case"
            let service = AuthService(context)
            // 1. Register UPPER
            do! service.RegisterAsync({ Email = "UPPER@test.com"; Password = "password123"; RememberMe = false }) |> Async.AwaitTask |> Async.Ignore
            // 2. Try lower
            let! result = service.RegisterAsync({ Email = "upper@test.com"; Password = "password123"; RememberMe = false })
            match result with | Error msg -> Assert.Contains("User already exists", msg) | Ok _ -> failwith "Should fail duplicate"
        }

    [<Fact>]
    member this.``RegisterAsync should fail if email already exists``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Reg_Duplicate"
            let service = AuthService(context)
            let dto = { Email = "exists@test.com"; Password = "password123"; RememberMe = false }
            do! service.RegisterAsync(dto) |> Async.AwaitTask |> Async.Ignore
            
            let! result = service.RegisterAsync(dto)
            match result with
            | Error msg -> msg |> should equal "User already exists"
            | Ok _ -> failwith "Should fail duplicate"
        }

    // --- ValidateUserAsync Tests ---

    [<Fact>]
    member this.``ValidateUserAsync should succeed with correct credentials``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Login_Success"
            let service = AuthService(context)
            let regDto = { Email = "user@test.com"; Password = "password123"; RememberMe = false }
            do! service.RegisterAsync(regDto) |> Async.AwaitTask |> Async.Ignore

            let loginDto : LoginDto = { Email = "user@test.com"; Password = "password123"; RememberMe = false }
            let! result = service.ValidateUserAsync(loginDto)
            
            match result with
            | Ok user -> user.Email |> should equal "user@test.com"
            | Error _ -> failwith "Login failed"
        }

    [<Fact>]
    member this.``ValidateUserAsync should fail with wrong password``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Login_WrongPass"
            let service = AuthService(context)
            let regDto = { Email = "user@test.com"; Password = "password123"; RememberMe = false }
            do! service.RegisterAsync(regDto) |> Async.AwaitTask |> Async.Ignore

            let loginDto : LoginDto = { Email = "user@test.com"; Password = "WRONG_PASSWORD"; RememberMe = false }
            let! result = service.ValidateUserAsync(loginDto)
            
            match result with
            | Error msg -> msg |> should equal "Invalid credentials"
            | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``ValidateUserAsync should fail when email is empty``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Login_EmailEmpty"
            let service = AuthService(context)
            let! result = service.ValidateUserAsync({ Email = ""; Password = "123"; RememberMe = false })
            match result with | Error msg -> Assert.Contains("required", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``ValidateUserAsync should fail when password is empty``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Login_PassEmpty"
            let service = AuthService(context)
            let! result = service.ValidateUserAsync({ Email = "a@a.com"; Password = ""; RememberMe = false })
            match result with | Error msg -> Assert.Contains("required", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``ValidateUserAsync should fail if email does not exist``() =
        task {
            use context = TestHelpers.getDbContext "Auth_Login_NoUser"
            let service = AuthService(context)
            let loginDto : LoginDto = { Email = "ghost@test.com"; Password = "any"; RememberMe = false }
            
            let! result = service.ValidateUserAsync(loginDto)
            match result with
            | Error msg -> msg |> should equal "Invalid credentials"
            | Ok _ -> failwith "Should fail"
        }