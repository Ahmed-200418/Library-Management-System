namespace LibraryManagementSystem.Tests.Services

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open LibraryManagementSystem.Services
open LibraryManagementSystem.Tests

type BookServiceTests() =

    [<Fact>]
    member this.``AddBookAsync should succeed and auto-generate ID``() =
        task {
            use context = TestHelpers.getDbContext "Book_Add_Success"
            let service = BookService(context)
            let book = TestHelpers.createBook 0 "Title" "Author"
            
            let! result = service.AddBookAsync(book)
            match result with
            | Ok saved -> 
                saved.Id |> should be (greaterThan 0)
                saved.IsBorrowed |> should be False
            | Error _ -> failwith "Should succeed"
        }

    [<Fact>]
    member this.``AddBookAsync should fail when title is empty``() =
        task {
            use context = TestHelpers.getDbContext "Book_Add_TitleEmpty"
            let service = BookService(context)
            let! result = service.AddBookAsync(TestHelpers.createBook 0 "" "Author")
            match result with | Error msg -> Assert.Contains("title is required", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``AddBookAsync should fail when author is empty``() =
        task {
            use context = TestHelpers.getDbContext "Book_Add_AuthorEmpty"
            let service = BookService(context)
            let! result = service.AddBookAsync(TestHelpers.createBook 0 "Title" "")
            match result with | Error msg -> Assert.Contains("author is required", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``AddBookAsync should always force IsBorrowed = false``() =
        task {
            use context = TestHelpers.getDbContext "Book_Add_ForceFalse"
            let service = BookService(context)
            let book = { TestHelpers.createBook 0 "T" "A" with IsBorrowed = true }
            let! result = service.AddBookAsync(book)
            match result with | Ok saved -> saved.IsBorrowed |> should be False | Error e -> failwith e
        }

    [<Fact>]
    member this.``UpdateBookAsync should update allowed fields only``() =
        task {
            use context = TestHelpers.getDbContext "Book_Update"
            let service = BookService(context)
            
            let! addResult = service.AddBookAsync(TestHelpers.createBook 0 "Original" "Author")
            let saved = match addResult with Ok b -> b | Error e -> failwith e

            // Attempt to hack IsBorrowed to true manually via Update
            let updateDto = { saved with Title = "Updated"; IsBorrowed = true } 
            
            let! result = service.UpdateBookAsync(saved.Id, updateDto)
            
            match result with
            | Ok () -> () 
            | Error e -> failwith $"Update failed: {e}"

            let! check = service.GetBookByIdAsync(saved.Id)
            match check with
            | Ok (Some b) -> 
                b.Title |> should equal "Updated"
                b.IsBorrowed |> should be False 
            | _ -> failwith "Book lookup failed"
        }

    [<Fact>]
    member this.``UpdateBookAsync should fail when title is empty``() =
        task {
            use context = TestHelpers.getDbContext "Book_Upd_TitleEmpty"
            let service = BookService(context)
            let! addRes = service.AddBookAsync(TestHelpers.createBook 0 "Old" "Auth")
            let saved = match addRes with Ok b -> b | Error e -> failwith e
            
            let! result = service.UpdateBookAsync(saved.Id, { saved with Title = "" })
            match result with | Error msg -> Assert.Contains("title is required", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``UpdateBookAsync should fail when author is empty``() =
        task {
            use context = TestHelpers.getDbContext "Book_Upd_AuthorEmpty"
            let service = BookService(context)
            let! addRes = service.AddBookAsync(TestHelpers.createBook 0 "Old" "Auth")
            let saved = match addRes with Ok b -> b | Error e -> failwith e
            
            let! result = service.UpdateBookAsync(saved.Id, { saved with Author = "" })
            match result with | Error msg -> Assert.Contains("author is required", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``DeleteBookAsync should fail if book is borrowed``() =
        task {
            use context = TestHelpers.getDbContext "Book_Del_Borrowed"
            let service = BookService(context)
            let book = TestHelpers.createBook 0 "Borrowed" "Auth"
            let borrowedBook = { book with IsBorrowed = true }
            context.Books.Add(borrowedBook) |> ignore
            context.SaveChanges() |> ignore

            let! result = service.DeleteBookAsync(borrowedBook.Id)
            match result with
            | Error msg -> Assert.Contains("borrowed", msg)
            | Ok _ -> failwith "Should fail deleting borrowed book"
        }

    [<Fact>]
    member this.``DeleteBookAsync should fail when book does not exist``() =
        task {
            use context = TestHelpers.getDbContext "Book_Del_NotExist"
            let service = BookService(context)
            let! result = service.DeleteBookAsync(9999)
            match result with | Error msg -> Assert.Contains("not found", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``SearchBooksAsync should be case-insensitive``() =
        task {
            use context = TestHelpers.getDbContext "Book_Search_Case"
            let service = BookService(context)
            do! service.AddBookAsync(TestHelpers.createBook 0 "F# Programming" "Don Syme") |> Async.AwaitTask |> Async.Ignore

            let! res = service.SearchBooksAsync("programming") 
            match res with
            | Ok books -> books |> Seq.length |> should equal 1
            | Error _ -> failwith "Search failed"
        }