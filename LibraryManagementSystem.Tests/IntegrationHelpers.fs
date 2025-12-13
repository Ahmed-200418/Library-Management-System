namespace LibraryManagementSystem.Tests

open System.Net.Http.Json
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.DependencyInjection
open LibraryManagementSystem.Data
open LibraryManagementSystem.Domain.Models 

module IntegrationHelpers =
    
    let getAuthenticatedClient (factory: CustomWebApplicationFactory) (email: string) (isAdmin: bool) =
        task {
            let options = Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions(AllowAutoRedirect = false)
            let client = factory.CreateClient(options)
            
            let password = if email = "admin@library.com" then "admin123927387893793" else "password123"

            // 1. Register
            let registerDto = {| Email = email; Password = password; RememberMe = false |}
            let! _ = client.PostAsJsonAsync("/api/auth/register", registerDto)

            // 2. Admin Upgrade (Manual DB manipulation)
            if isAdmin then
                use scope = factory.Services.CreateScope()
                let db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>()
                let user = db.Users |> Seq.find (fun u -> u.Email = email)
                let updatedUser = { user with Role = "Admin" }
                db.Entry(user).CurrentValues.SetValues(updatedUser)
                db.SaveChanges() |> ignore

            // 3. Login
            let loginDto = {| Email = email; Password = password; RememberMe = false |}
            let! response = client.PostAsJsonAsync("/api/auth/login", loginDto)
            
            if response.StatusCode <> System.Net.HttpStatusCode.OK then
                failwithf "Login failed for %s" email

            return client
        }