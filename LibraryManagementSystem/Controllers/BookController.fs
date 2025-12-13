namespace LibraryManagementSystem.Controllers

open System
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open LibraryManagementSystem.Services
open LibraryManagementSystem.Domain.Models

module BookController =

    let Register (app: WebApplication) =
        // GET All Books
        app.MapGet("/api/books", Func<BookService, Task<IResult>>(fun service ->
            task {
                let! result = service.GetAllBooksAsync()
                match result with
                | Ok books -> return Results.Ok(books)
                | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization() |> ignore

        // GET Single Book
        app.MapGet("/api/books/{id}", Func<int, BookService, Task<IResult>>(fun id service ->
            task {
                let! result = service.GetBookByIdAsync(id)
                match result with
                | Ok (Some b) -> return Results.Ok(b)
                | Ok None -> return Results.NotFound()
                | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization() |> ignore

        // POST Add Book (handles image upload)
        app.MapPost("/api/books", Func<HttpRequest, BookService, Task<IResult>>(fun req service ->
            task {
                let form = req.Form
                let file = if form.Files.Count > 0 then form.Files.[0] else null
                let mutable imagePath = ""
                if not (isNull file) then
                    let fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName)
                    let path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName)
                    use stream = new FileStream(path, FileMode.Create)
                    do! file.CopyToAsync(stream)
                    imagePath <- "/images/" + fileName

                let book = {
                    Id = 0
                    Title = form.["title"].ToString()
                    Author = form.["author"].ToString()
                    Description = form.["description"].ToString()
                    ImagePath = imagePath
                    IsBorrowed = false
                    CreatedAt = DateTime.UtcNow
                }
                let! result = service.AddBookAsync(book)
                match result with
                | Ok saved -> return Results.Ok(saved)
                | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization(fun p -> p.RequireRole("Admin") |> ignore) |> ignore

        // PUT Edit Book
        app.MapPut("/api/books/{id}", Func<int, Book, BookService, Task<IResult>>(fun id book service ->
            task {
                if id <> book.Id then return Results.BadRequest("ID mismatch")
                else
                    let! result = service.UpdateBookAsync(id, book)
                    match result with
                    | Ok () -> return Results.Ok()
                    | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization(fun p -> p.RequireRole("Admin") |> ignore) |> ignore

        // POST Update Book with Image
        app.MapPost("/api/books/{id}/update-with-image", Func<int, HttpRequest, BookService, Task<IResult>>(fun id req service ->
            task {
                let form = req.Form
                let file = if form.Files.Count > 0 then form.Files.["image"] else null
                let mutable imagePath = ""
                let! existingBookResult = service.GetBookByIdAsync(id)
                match existingBookResult with
                | Error msg -> return Results.BadRequest(msg)
                | Ok None -> return Results.NotFound()
                | Ok (Some existing) ->
                    if not (isNull file) && file.Length > 0L then
                        let oldImagePath = form.["oldImagePath"].ToString()
                        if not (String.IsNullOrEmpty(oldImagePath)) then
                            do! service.DeleteImageAsync(oldImagePath)

                        let fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName)
                        let path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName)
                        use stream = new FileStream(path, FileMode.Create)
                        do! file.CopyToAsync(stream)
                        imagePath <- "/images/" + fileName
                    else
                        imagePath <- existing.ImagePath

                    let updatedBook = {
                        Id = id
                        Title = form.["title"].ToString()
                        Author = form.["author"].ToString()
                        Description = form.["description"].ToString()
                        ImagePath = imagePath
                        IsBorrowed = existing.IsBorrowed
                        CreatedAt = existing.CreatedAt
                    }
                    let! result = service.UpdateBookAsync(id, updatedBook)
                    match result with
                    | Ok () -> return Results.Ok()
                    | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization(fun p -> p.RequireRole("Admin") |> ignore) |> ignore

        // DELETE Book
        app.MapDelete("/api/books/{id}", Func<int, BookService, Task<IResult>>(fun id s ->
            task {
                let! result = s.DeleteBookAsync(id)
                match result with
                | Ok () -> return Results.Ok()
                | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization(fun p -> p.RequireRole("Admin") |> ignore) |> ignore

        // GET Search Books
        app.MapGet("/api/books/search/{query}", Func<string, BookService, Task<IResult>>(fun query service ->
            task {
                let! result = service.SearchBooksAsync(query)
                match result with
                | Ok books -> return Results.Ok(books)
                | Error msg -> return Results.BadRequest(msg)
            }
        )).RequireAuthorization() |> ignore

        app