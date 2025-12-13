namespace LibraryManagementSystem.Tests.Controllers

open System.Net
open System.Net.Http.Json
open Xunit
open FsUnit.Xunit
open LibraryManagementSystem.Tests

type AuthControllerTests(factory: CustomWebApplicationFactory) =
    interface IClassFixture<CustomWebApplicationFactory>

    [<Fact>]
    member this.``POST /register should create user``() =
        task {
            let client = factory.CreateClient()
            let dto = {| Email = "int_new@test.com"; Password = "password123"; RememberMe = false |}
            let! response = client.PostAsJsonAsync("/api/auth/register", dto)
            response.StatusCode |> should not' (equal HttpStatusCode.BadRequest)
        }

    [<Fact>]
    member this.``POST /login should set Auth Cookie``() =
        task {
            let client = factory.CreateClient()
            let dto = {| Email = "cookie@test.com"; Password = "password123"; RememberMe = false |}
            do! client.PostAsJsonAsync("/api/auth/register", dto) |> Async.AwaitTask |> Async.Ignore

            let! response = client.PostAsJsonAsync("/api/auth/login", dto)
            response.StatusCode |> should equal HttpStatusCode.OK
            response.Headers.Contains("Set-Cookie") |> should be True
        }