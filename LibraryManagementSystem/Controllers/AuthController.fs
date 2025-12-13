namespace LibraryManagementSystem.Controllers

open System
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open LibraryManagementSystem.Services
open LibraryManagementSystem.Domain.DTOs

module AuthController =

    let Register (app: WebApplication) =
        // Register auth endpoints
        app.MapPost("/api/auth/register", Func<HttpContext, AuthService, RegisterDto, Task<IResult>>(fun http service dto ->
            task {
                let! result = service.RegisterAsync(dto)
                match result with
                | Ok user ->
                    let claims = [|
                        Claim(ClaimTypes.NameIdentifier, string user.Id)
                        Claim(ClaimTypes.Name, user.Email)
                        Claim(ClaimTypes.Role, user.Role)
                    |]
                    let identity = ClaimsIdentity(claims, "Cookies")
                    let props = 
                        let p = Microsoft.AspNetCore.Authentication.AuthenticationProperties(IsPersistent = dto.RememberMe)
                        if dto.RememberMe then p.ExpiresUtc <- DateTimeOffset.UtcNow.AddDays(7.0)
                        p
                    do! http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ClaimsPrincipal(identity), props)
                    return Results.Redirect("/index.html")
                | Error msg -> return Results.BadRequest(msg)
            }
        )) |> ignore

        app.MapPost("/api/auth/login", Func<HttpContext, AuthService, LoginDto, Task<IResult>>(fun http service dto ->
            task {
                let! result = service.ValidateUserAsync(dto)
                match result with
                | Ok user ->
                    let claims = [|
                        Claim(ClaimTypes.NameIdentifier, string user.Id)
                        Claim(ClaimTypes.Name, user.Email)
                        Claim(ClaimTypes.Role, user.Role)
                    |]
                    let identity = ClaimsIdentity(claims, "Cookies")
                    let props =
                        let p = Microsoft.AspNetCore.Authentication.AuthenticationProperties(IsPersistent = dto.RememberMe)
                        if dto.RememberMe then p.ExpiresUtc <- DateTimeOffset.UtcNow.AddDays(7.0)
                        p
                    do! http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ClaimsPrincipal(identity), props)
                    return Results.Ok({| Role = user.Role |})
                | Error msg -> return Results.Unauthorized()
            }
        )) |> ignore

        app.MapPost("/api/auth/logout", Func<HttpContext, Task<IResult>>(fun http ->
            task {
                do! http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                return Results.Ok()
            }
        )) |> ignore

        app