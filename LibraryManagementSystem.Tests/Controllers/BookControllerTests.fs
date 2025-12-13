namespace LibraryManagementSystem.Tests.Controllers

open System.Net
open System.Net.Http
open Xunit
open FsUnit.Xunit
open Microsoft.Extensions.DependencyInjection
open LibraryManagementSystem.Domain.Models
open LibraryManagementSystem.Tests 

type BookControllerTests(factory: CustomWebApplicationFactory) =
    interface IClassFixture<CustomWebApplicationFactory>

    [<Fact>]
    member this.``GET /api/books should authorize authenticated user``() =
        task {
            let! client = IntegrationHelpers.getAuthenticatedClient factory "reader@test.com" false
            let! response = client.GetAsync("/api/books")
            response.StatusCode |> should equal HttpStatusCode.OK
        }

    [<Fact>]
    member this.``DELETE /api/books/{id} should forbid non-admin``() =
        task {
            let! client = IntegrationHelpers.getAuthenticatedClient factory "regular@test.com" false
            let! response = client.DeleteAsync("/api/books/1")
            
            let isForbidden = response.StatusCode = HttpStatusCode.Forbidden || response.StatusCode = HttpStatusCode.Found
            isForbidden |> should be True
        }

    [<Fact>]
    member this.``DELETE /api/books/{id} should allow Admin``() =
        task {
            let! client = IntegrationHelpers.getAuthenticatedClient factory "admin@library.com" true
            
            let bookId = 
                task {
                    use scope = factory.Services.CreateScope()
                    let service = scope.ServiceProvider.GetRequiredService<LibraryManagementSystem.Services.BookService>()
                    let! res = service.AddBookAsync(TestHelpers.createBook 0 "DelMe" "A")
                    return match res with Ok b -> b.Id | Error _ -> 0
                }
            let! id = bookId

            let! response = client.DeleteAsync($"/api/books/{id}")
            response.StatusCode |> should equal HttpStatusCode.OK
        }