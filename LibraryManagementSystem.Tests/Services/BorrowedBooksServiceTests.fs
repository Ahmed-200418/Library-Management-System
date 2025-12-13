namespace LibraryManagementSystem.Tests.Services

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open LibraryManagementSystem.Services
open LibraryManagementSystem.Tests

type BorrowedBooksServiceTests() =
    
    [<Fact>]
    member this.``BorrowBookAsync should succeed and update IsBorrowed``() =
        task {
            use context = TestHelpers.getDbContext "BB_Borrow_Happy"
            let bs = BookService(context)
            let bbs = BorrowedBooksService(context)
            
            let! addResult = bs.AddBookAsync(TestHelpers.createBook 0 "T" "A")
            let book = match addResult with Ok b -> b | Error e -> failwith e

            let! result = bbs.BorrowBookAsync(1, book.Id)
            match result with
            | Ok () -> ()
            | Error e -> failwith $"Borrow failed: {e}"

            let! checkBookRes = bs.GetBookByIdAsync(book.Id)
            match checkBookRes with
            | Ok (Some b) -> b.IsBorrowed |> should be True
            | _ -> failwith "Book not found"
        }

    [<Fact>]
    member this.``BorrowBookAsync should fail if book not found``() =
        task {
            use context = TestHelpers.getDbContext "BB_Borrow_NoBook"
            let bbs = BorrowedBooksService(context)
            let! result = bbs.BorrowBookAsync(1, 999)
            
            match result with 
            | Error msg -> Assert.Contains("not found", msg)
            | _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``BorrowBookAsync should fail when userId is invalid``() =
        task {
            use context = TestHelpers.getDbContext "BB_InvalidUser"
            let bbs = BorrowedBooksService(context)
            let! result = bbs.BorrowBookAsync(0, 1)
            match result with | Error msg -> Assert.Contains("Invalid user", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``BorrowBookAsync should fail when bookId is invalid``() =
        task {
            use context = TestHelpers.getDbContext "BB_InvalidBook"
            let bbs = BorrowedBooksService(context)
            let! result = bbs.BorrowBookAsync(1, 0)
            match result with | Error msg -> Assert.Contains("Invalid book", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``BorrowBookAsync should fail if already borrowed``() =
        task {
            use context = TestHelpers.getDbContext "BB_Borrow_Duplicate"
            let bs = BookService(context)
            let bbs = BorrowedBooksService(context)
            
            let! addResult = bs.AddBookAsync(TestHelpers.createBook 0 "T" "A")
            let book = match addResult with Ok b -> b | Error e -> failwith e
            
            do! bbs.BorrowBookAsync(1, book.Id) |> Async.AwaitTask |> Async.Ignore

            let! result = bbs.BorrowBookAsync(2, book.Id)
            match result with 
            | Error msg -> Assert.Contains("already borrowed", msg)
            | _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``BorrowBookAsync should not allow same user to borrow same book twice``() =
        task {
            use context = TestHelpers.getDbContext "BB_SameUserTwice"
            let bs = BookService(context)
            let bbs = BorrowedBooksService(context)
            let! addRes = bs.AddBookAsync(TestHelpers.createBook 0 "T" "A")
            let book = match addRes with Ok b -> b | Error e -> failwith e

            // First Borrow
            do! bbs.BorrowBookAsync(1, book.Id) |> Async.AwaitTask |> Async.Ignore
            
            // Second Borrow (Same User)
            let! result = bbs.BorrowBookAsync(1, book.Id)
            match result with | Error msg -> Assert.Contains("already borrowed", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``ReturnBookAsync should succeed and release book``() =
        task {
            use context = TestHelpers.getDbContext "BB_Return_Happy"
            let bs = BookService(context)
            let bbs = BorrowedBooksService(context)
            
            let! addResult = bs.AddBookAsync(TestHelpers.createBook 0 "T" "A")
            let book = match addResult with Ok b -> b | Error e -> failwith e

            do! bbs.BorrowBookAsync(1, book.Id) |> Async.AwaitTask |> Async.Ignore

            let! result = bbs.ReturnBookAsync(1, book.Id)
            match result with
            | Ok () -> ()
            | Error e -> failwith $"Return failed: {e}"

            let! checkBookRes = bs.GetBookByIdAsync(book.Id)
            match checkBookRes with
            | Ok (Some b) -> b.IsBorrowed |> should be False
            | _ -> failwith "Book not found"
        }

    [<Fact>]
    member this.``ReturnBookAsync should fail if trying to return someone else's book``() =
        task {
            use context = TestHelpers.getDbContext "BB_Return_WrongUser"
            let bs = BookService(context)
            let bbs = BorrowedBooksService(context)
            
            let! addResult = bs.AddBookAsync(TestHelpers.createBook 0 "T" "A")
            let book = match addResult with Ok b -> b | Error e -> failwith e
            
            do! bbs.BorrowBookAsync(1, book.Id) |> Async.AwaitTask |> Async.Ignore

            let! result = bbs.ReturnBookAsync(2, book.Id)
            match result with 
            | Error msg -> Assert.Contains("No active borrow", msg)
            | _ -> failwith "Should fail security check"
        }

    [<Fact>]
    member this.``ReturnBookAsync should fail if book does not exist``() =
        task {
            use context = TestHelpers.getDbContext "BB_Return_NotExist"
            let bbs = BorrowedBooksService(context)
            let! result = bbs.ReturnBookAsync(1, 9999)
            match result with | Error msg -> Assert.Contains("Book not found", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``ReturnBookAsync should fail if book is already returned``() =
        task {
            use context = TestHelpers.getDbContext "BB_Return_AlreadyReturned"
            let bs = BookService(context)
            let bbs = BorrowedBooksService(context)
            let! addRes = bs.AddBookAsync(TestHelpers.createBook 0 "T" "A")
            let book = match addRes with Ok b -> b | Error e -> failwith e

            // Borrow and Return once
            do! bbs.BorrowBookAsync(1, book.Id) |> Async.AwaitTask |> Async.Ignore
            do! bbs.ReturnBookAsync(1, book.Id) |> Async.AwaitTask |> Async.Ignore

            // Try Return again
            let! result = bbs.ReturnBookAsync(1, book.Id)
            match result with | Error msg -> Assert.Contains("No active borrow", msg) | Ok _ -> failwith "Should fail"
        }

    [<Fact>]
    member this.``GetUserBorrowedBooksAsync should return only active borrows``() =
        task {
            use context = TestHelpers.getDbContext "BB_GetActive"
            let bs = BookService(context)
            let bbs = BorrowedBooksService(context)
            
            // Book 1: Borrowed and Returned (Inactive)
            let! b1 = bs.AddBookAsync(TestHelpers.createBook 0 "B1" "A")
            let book1 = match b1 with Ok b -> b | Error _ -> Unchecked.defaultof<_>
            do! bbs.BorrowBookAsync(1, book1.Id) |> Async.AwaitTask |> Async.Ignore
            do! bbs.ReturnBookAsync(1, book1.Id) |> Async.AwaitTask |> Async.Ignore

            // Book 2: Borrowed (Active)
            let! b2 = bs.AddBookAsync(TestHelpers.createBook 0 "B2" "A")
            let book2 = match b2 with Ok b -> b | Error _ -> Unchecked.defaultof<_>
            do! bbs.BorrowBookAsync(1, book2.Id) |> Async.AwaitTask |> Async.Ignore

            let! result = bbs.GetUserBorrowedBooksAsync(1)
            match result with
            | Ok list -> 
                list |> Seq.length |> should equal 1
                list |> Seq.head |> fun x -> x.BookId |> should equal book2.Id
            | Error e -> failwith e
        }

    [<Fact>]
    member this.``GetBookBorrowHistoryAsync should order by date desc``() =
        task {
            use context = TestHelpers.getDbContext "BB_History_Order"
            let bs = BookService(context)
            let bbs = BorrowedBooksService(context)
            
            let! addResult = bs.AddBookAsync(TestHelpers.createBook 0 "T" "A")
            let book = match addResult with Ok b -> b | Error e -> failwith e
            
            // 1. Borrow and Return (Old)
            do! bbs.BorrowBookAsync(1, book.Id) |> Async.AwaitTask |> Async.Ignore
            do! bbs.ReturnBookAsync(1, book.Id) |> Async.AwaitTask |> Async.Ignore
            
            // 2. Borrow (New)
            do! bbs.BorrowBookAsync(2, book.Id) |> Async.AwaitTask |> Async.Ignore

            let! res = bbs.GetBookBorrowHistoryAsync(book.Id)
            match res with // Fixed: Match on 'res', not 'result'
            | Ok history ->
                history |> Seq.length |> should equal 2
                let list = history |> Seq.toList
                list.[0].BorrowDate |> should be (greaterThanOrEqualTo list.[1].BorrowDate)
            | Error _ -> failwith "History failed"
        }

    [<Fact>]
    member this.``GetBookBorrowHistoryAsync should fail if book does not exist``() =
        task {
            use context = TestHelpers.getDbContext "BB_Hist_NotExist"
            let bbs = BorrowedBooksService(context)
            let! result = bbs.GetBookBorrowHistoryAsync(9999)
            match result with | Error msg -> Assert.Contains("not found", msg) | Ok _ -> failwith "Should fail"
        }