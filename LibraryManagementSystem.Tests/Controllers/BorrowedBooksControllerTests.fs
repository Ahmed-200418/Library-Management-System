namespace LibraryManagementSystem.Tests.Controllers

open System.Net
open Xunit
open FsUnit.Xunit
open Microsoft.Extensions.DependencyInjection
open LibraryManagementSystem.Domain.Models
open LibraryManagementSystem.Tests 

type BorrowedBooksControllerTests(factory: CustomWebApplicationFactory) =
    interface IClassFixture<CustomWebApplicationFactory>

    [<Fact>]
    member this.``POST /borrow should return Unauthorized if not logged in``() =
        task {
            let client = factory.CreateClient(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions(AllowAutoRedirect = false))
            let! response = client.PostAsync("/api/books/1/borrow", null)
            
            let isAuthError = response.StatusCode = HttpStatusCode.Unauthorized || response.StatusCode = HttpStatusCode.Found
            isAuthError |> should be True
        }