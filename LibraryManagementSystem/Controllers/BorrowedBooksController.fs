namespace LibraryManagementSystem.Controllers

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System.Security.Claims
open LibraryManagementSystem.Services

module BorrowedBooksController =

    let Register (app: WebApplication) =
        // Borrow Book
        app.MapPost("/api/books/{id}/borrow", Func<HttpContext, int, BorrowedBooksService, Task<IResult>>(fun http id s ->
            task {
                let userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)
                if isNull userIdClaim then
                    return Results.Unauthorized()
                else
                    let userId = int userIdClaim.Value
                    let! result = s.BorrowBookAsync(userId, id)
                    match result with
                    | Ok () -> return Results.Ok()
                    | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization() |> ignore

        // Return Book
        app.MapPost("/api/books/{id}/return", Func<HttpContext, int, BorrowedBooksService, Task<IResult>>(fun http id s ->
            task {
                let userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)
                if isNull userIdClaim then
                    return Results.Unauthorized()
                else
                    let userId = int userIdClaim.Value
                    let! result = s.ReturnBookAsync(userId, id)
                    match result with
                    | Ok () -> return Results.Ok()
                    | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization() |> ignore

        // Check if user has borrowed a book
        app.MapGet("/api/books/{id}/is-borrowed-by-user", Func<HttpContext, int, BorrowedBooksService, Task<IResult>>(fun http id s ->
            task {
                let userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)
                if isNull userIdClaim then
                    return Results.Unauthorized()
                else
                    let userId = int userIdClaim.Value
                    let! result = s.HasUserBorrowedBookAsync(userId, id)
                    match result with
                    | Ok hasBorrowed -> return Results.Ok({| hasBorrowed = hasBorrowed |})
                    | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization() |> ignore

        app