namespace LibraryManagementSystem.Tests

open System
open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Hosting
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open LibraryManagementSystem.Data
open LibraryManagementSystem.Domain.Models 

module TestHelpers =
    // --- Helper for UNIT TESTS (In-Memory) ---
    let getDbContext (dbName: string) =
        let options = 
            DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName = dbName)
                .Options
        let context = new LibraryDbContext(options)
        context.Database.EnsureDeleted() |> ignore
        context.Database.EnsureCreated() |> ignore
        context

    // --- Helper for DATA CREATION ---
    let createBook id t a = 
        { Id = id; Title = t; Author = a; Description = "Desc"; ImagePath = ""; IsBorrowed = false; CreatedAt = DateTime.UtcNow }

// --- Factory for INTEGRATION TESTS (SQL LocalDB) ---
type CustomWebApplicationFactory() =
    inherit WebApplicationFactory<LibraryDbContext>()

    // Unique Database per Test Class to avoid collisions
    let dbName = "LibraryDb_TEST_" + Guid.NewGuid().ToString("N")
    let connectionString = sprintf "Server=(localdb)\\mssqllocaldb;Database=%s;Trusted_Connection=True;MultipleActiveResultSets=true" dbName

    override this.CreateHost(builder: IHostBuilder) =
        let host = base.CreateHost(builder)
        use scope = host.Services.CreateScope()
        let db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>()
        db.Database.EnsureDeleted() |> ignore
        db.Database.EnsureCreated() |> ignore
        host

    override this.ConfigureWebHost(builder: IWebHostBuilder) =
        builder.ConfigureServices(fun (services: IServiceCollection) ->
            // Replace Real DB with Test DB
            services.RemoveAll(typeof<DbContextOptions<LibraryDbContext>>) |> ignore
            services.RemoveAll(typeof<LibraryDbContext>) |> ignore
            services.AddDbContext<LibraryDbContext>(fun options -> 
                options.UseSqlServer(connectionString) |> ignore) |> ignore

            // Relax Cookie Security
            services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, fun (options: CookieAuthenticationOptions) ->
                options.Cookie.SecurePolicy <- CookieSecurePolicy.SameAsRequest
                options.Cookie.SameSite <- SameSiteMode.Lax
            ) |> ignore
        ) |> ignore

    override this.Dispose(disposing: bool) =
        if disposing then
            try
                let optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>()
                optionsBuilder.UseSqlServer(connectionString) |> ignore
                using (new LibraryDbContext(optionsBuilder.Options)) (fun db -> 
                    db.Database.EnsureDeleted() |> ignore
                )
            with _ -> ()
        base.Dispose(disposing)