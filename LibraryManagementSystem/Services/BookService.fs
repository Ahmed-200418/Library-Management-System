namespace LibraryManagementSystem.Services

open System
open System.Linq
open System.Threading.Tasks
open Microsoft.EntityFrameworkCore
open LibraryManagementSystem.Data
open LibraryManagementSystem.Domain.Models

type BookService(context: LibraryDbContext) =

    // Helper method to delete image file
    member this.DeleteImageAsync (imagePath: string) =
        async {
            try
                if not (String.IsNullOrEmpty(imagePath)) && imagePath.StartsWith("/images/") then
                    let fileName = imagePath.Replace("/images/", "")
                    let path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", fileName)
                    if System.IO.File.Exists(path) then
                        System.IO.File.Delete(path)
                return ()
            with _ -> return ()
        } |> Async.StartAsTask

    // Get all books
    member this.GetAllBooksAsync () =
        async {
            try
                let! books = Async.AwaitTask (context.Books.ToListAsync())
                return Ok books
            with ex ->
                return Error $"Failed to retrieve books: {ex.Message}"
        } |> Async.StartAsTask

    // Get single book by ID
    member this.GetBookByIdAsync (id: int) =
        async {
            try
                if id <= 0 then
                    return Error "Invalid book ID"
                else
                    let! book = context.Books.FindAsync(id).AsTask() |> Async.AwaitTask
                    if isNull (box book) then
                        return Error "Book not found"
                    else
                        return Ok (Some book)
            with ex ->
                return Error $"Failed to retrieve book: {ex.Message}"
        } |> Async.StartAsTask

    // Add a new book
    member this.AddBookAsync (book: Book) =
        async {
            try
                if String.IsNullOrWhiteSpace(book.Title) then
                    return Error "Book title is required"
                else if String.IsNullOrWhiteSpace(book.Author) then
                    return Error "Book author is required"
                else
                    // FORCE IsBorrowed = false, Id = 0 (Auto-gen)
                    let newBook = { book with IsBorrowed = false; Id = 0 }
                    
                    context.Books.Add(newBook) |> ignore
                    let! _ = Async.AwaitTask (context.SaveChangesAsync())
                    return Ok newBook
            with ex ->
                return Error $"Failed to add book: {ex.Message}"
        } |> Async.StartAsTask

    // Update an existing book
    member this.UpdateBookAsync (id: int, updatedBook: Book) =
        async {
            try
                if id <= 0 then 
                    return Error "Invalid book ID"
                else if String.IsNullOrWhiteSpace(updatedBook.Title) then 
                    return Error "Book title is required"
                else if String.IsNullOrWhiteSpace(updatedBook.Author) then 
                    return Error "Book author is required"
                else
                    let! existingBook = context.Books.FindAsync(id).AsTask() |> Async.AwaitTask
                    if isNull (box existingBook) then 
                        return Error "Book not found"
                    else
                        let bookToSave = {
                            existingBook with
                                Title = updatedBook.Title
                                Author = updatedBook.Author
                                Description = updatedBook.Description
                                ImagePath = updatedBook.ImagePath
                                // Do NOT update IsBorrowed here
                        }

                        context.Entry(existingBook).CurrentValues.SetValues(bookToSave)
                        let! _ = Async.AwaitTask (context.SaveChangesAsync())
                        return Ok ()
            with ex ->
                return Error $"Failed to update book: {ex.Message}"
        } |> Async.StartAsTask

    // Delete a book
    member this.DeleteBookAsync (id: int) =
        async {
            try
                if id <= 0 then 
                    return Error "Invalid book ID"
                else
                    let! book = context.Books.FindAsync(id).AsTask() |> Async.AwaitTask
                    if isNull (box book) then 
                        return Error "Book not found"
                    else if book.IsBorrowed then 
                        return Error "Cannot delete a book that is currently borrowed"
                    else
                        context.Books.Remove(book) |> ignore
                        let! _ = Async.AwaitTask (context.SaveChangesAsync())
                        return Ok ()
            with ex ->
                return Error $"Failed to delete book: {ex.Message}"
        } |> Async.StartAsTask

    // Search books by title or author
    member this.SearchBooksAsync (query: string) =
        async {
            try
                if String.IsNullOrWhiteSpace(query) then
                    let! books = Async.AwaitTask (context.Books.ToListAsync())
                    return Ok books
                else
                    // Trim and lower
                    let lowerQuery = query.ToLower().Trim()
                    let! books =
                        Async.AwaitTask (
                            context.Books
                                .Where(fun b ->
                                    b.Title.ToLower().Contains(lowerQuery) ||
                                    b.Author.ToLower().Contains(lowerQuery))
                                .ToListAsync()
                        )
                    return Ok books
            with ex ->
                return Error $"Failed to search books: {ex.Message}"
        } |> Async.StartAsTask