namespace LibraryManagementSystem

open System
open System.IO
open System.Threading.Tasks
open System.Security.Claims
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open LibraryManagementSystem.Data
open LibraryManagementSystem.Services
open LibraryManagementSystem.Domain.DTOs
open LibraryManagementSystem.Domain.Models
open LibraryManagementSystem.Services
open LibraryManagementSystem.Controllers

module Program =
    let [<EntryPoint>] main args =
        let builder = WebApplication.CreateBuilder(args)

        // -------------------------
        // 1. Service Configuration
        // -------------------------
        
        // Database Context
        builder.Services.AddDbContext<LibraryDbContext>(fun options ->
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")) |> ignore) |> ignore

        // Custom Services
        builder.Services.AddScoped<AuthService>() |> ignore
        builder.Services.AddScoped<BookService>() |> ignore
        builder.Services.AddScoped<BorrowedBooksService>() |> ignore
        builder.Services.AddAuthorization() |> ignore

        // Cookie Authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(fun options ->
                options.Cookie.Name <- "LibraryAuth"
                options.Cookie.HttpOnly <- true
                options.Cookie.SecurePolicy <- CookieSecurePolicy.Always // HTTPS only
                options.Cookie.SameSite <- SameSiteMode.Strict // Strict for security
                options.LoginPath <- "/login.html" // Redirect here if not logged in
                options.LogoutPath <- "/api/auth/logout"
            ) |> ignore

        let app = builder.Build()

        // -------------------------
        // 2. Middleware Pipeline
        // -------------------------
        
        if app.Environment.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore
        else
            app.UseHsts() |> ignore
            app.UseHttpsRedirection() |> ignore

        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore

        // Custom middleware to handle root and login page access
        app.Use(fun (context: HttpContext) (next: RequestDelegate) ->
            let path = context.Request.Path.Value
            let isAuthenticated = context.User.Identity.IsAuthenticated

            let lowerPath = if path <> null then path.ToLowerInvariant() else ""

            if lowerPath = "/" then
                if isAuthenticated then
                    context.Response.Redirect("/index.html")
                    Task.CompletedTask
                else
                    context.Response.Redirect("/login.html")
                    Task.CompletedTask
            elif lowerPath = "/login.html" && isAuthenticated then
                context.Response.Redirect("/index.html")
                Task.CompletedTask
            else
                next.Invoke(context)
        ) |> ignore

        app.UseStaticFiles() |> ignore // Serves wwwroot (HTML/CSS/JS)

        // Register API endpoints via controllers
        AuthController.Register(app) |> ignore
        BookController.Register(app) |> ignore
        BorrowedBooksController.Register(app) |> ignore

        // -------------------------
        // 4. Database Initialization
        // -------------------------
        use scope = app.Services.CreateScope()
        let db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>()
        
        // AUTO-CREATE DATABASE (Only on first run)
        db.Database.EnsureCreated() |> ignore 
        
        // SEED ADMIN USER
        let exists = db.Users.AnyAsync(fun u -> u.Email = "admin@library.com").Result
        if not exists then
            let admin = {
                Id=0
                Email="admin@library.com"
                PasswordHash=BCrypt.Net.BCrypt.HashPassword("admin123927387893793")
                Role="Admin"
                CreatedAt=DateTime.UtcNow
            }
            db.Users.Add(admin) |> ignore
            db.SaveChanges() |> ignore

        app.Run()
        0