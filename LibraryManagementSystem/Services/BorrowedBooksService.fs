namespace LibraryManagementSystem.Services

open System
open System.Linq
open System.Threading.Tasks
open Microsoft.EntityFrameworkCore
open LibraryManagementSystem.Data
open LibraryManagementSystem.Domain.Models

type BorrowedBooksService(context: LibraryDbContext) =

    member this.HasUserBorrowedBookAsync(userId: int, bookId: int) =
        async {
            try
                if userId <= 0 then 
                    return Error "Invalid user ID"
                else
                    let! borrowed = Async.AwaitTask (context.BorrowedBooks.FirstOrDefaultAsync(fun bb -> bb.UserId = userId && bb.BookId = bookId && bb.ReturnDate = Nullable()))
                    return Ok (not (isNull (box borrowed)))
            with ex ->
                return Error $"Failed to check borrow status: {ex.Message}"
        } |> Async.StartAsTask

    member this.GetUserBorrowedBooksAsync(userId: int) =
        async {
            try
                if userId <= 0 then 
                    return Error "Invalid user ID"
                else
                    let! books = Async.AwaitTask (context.BorrowedBooks.Where(fun bb -> bb.UserId = userId && bb.ReturnDate = Nullable()).ToListAsync())
                    return Ok books
            with ex ->
                return Error $"Failed to retrieve user borrowed books: {ex.Message}"
        } |> Async.StartAsTask

    member this.BorrowBookAsync(userId: int, bookId: int) =
        async {
            try
                if userId <= 0 then 
                    return Error "Invalid user ID"
                else if bookId <= 0 then 
                    return Error "Invalid book ID"
                else
                    let! book = context.Books.FindAsync(bookId).AsTask() |> Async.AwaitTask
                    if isNull (box book) then 
                        return Error "Book not found"
                    else
                        // Check active borrows for this book
                        let! existingBorrow = Async.AwaitTask (context.BorrowedBooks.FirstOrDefaultAsync(fun bb -> bb.BookId = bookId && bb.ReturnDate = Nullable()))
                        
                        if not (isNull (box existingBorrow)) then 
                            if existingBorrow.UserId = userId then
                                return Error "User has already borrowed this book"
                            else
                                return Error "Book is already borrowed"
                        else
                            let borrowRecord = { Id = 0; UserId = userId; BookId = bookId; BorrowDate = DateTime.UtcNow; ReturnDate = Nullable() }
                            context.BorrowedBooks.Add(borrowRecord) |> ignore
                            
                            // Update book status
                            let updatedBook = { book with IsBorrowed = true }
                            context.Entry(book).CurrentValues.SetValues(updatedBook)
                            
                            let! _ = Async.AwaitTask (context.SaveChangesAsync())
                            return Ok ()
            with ex ->
                return Error $"Failed to borrow book: {ex.Message}"
        } |> Async.StartAsTask

    member this.ReturnBookAsync(userId: int, bookId: int) =
        async {
            try
                if userId <= 0 then 
                    return Error "Invalid user ID"
                else if bookId <= 0 then 
                    return Error "Invalid book ID"
                else
                    // Check Book Existence First
                    let! book = context.Books.FindAsync(bookId).AsTask() |> Async.AwaitTask
                    if isNull (box book) then 
                        return Error "Book not found"
                    else
                        // Check Borrow Record
                        let! borrowRecord = Async.AwaitTask (context.BorrowedBooks.FirstOrDefaultAsync(fun bb -> bb.UserId = userId && bb.BookId = bookId && bb.ReturnDate = Nullable()))
                        if isNull (box borrowRecord) then 
                            return Error "No active borrow record found for this user and book"
                        else
                            // Update borrow record
                            let updatedRecord = { borrowRecord with ReturnDate = Nullable(DateTime.UtcNow) }
                            context.Entry(borrowRecord).CurrentValues.SetValues(updatedRecord)
                            
                            // Update book status
                            let updatedBook = { book with IsBorrowed = false }
                            context.Entry(book).CurrentValues.SetValues(updatedBook)
                            
                            let! _ = Async.AwaitTask (context.SaveChangesAsync())
                            return Ok ()
            with ex ->
                return Error $"Failed to return book: {ex.Message}"
        } |> Async.StartAsTask

    member this.GetBookBorrowHistoryAsync(bookId: int) =
        async {
            try
                if bookId <= 0 then 
                    return Error "Invalid book ID"
                else
                    let! book = context.Books.FindAsync(bookId).AsTask() |> Async.AwaitTask
                    if isNull (box book) then 
                        return Error "Book not found"
                    else
                        let! history = Async.AwaitTask (context.BorrowedBooks.Where(fun bb -> bb.BookId = bookId).OrderByDescending(fun bb -> bb.BorrowDate).ToListAsync())
                        return Ok history
            with ex ->
                return Error $"Failed to retrieve book borrow history: {ex.Message}"
        } |> Async.StartAsTask