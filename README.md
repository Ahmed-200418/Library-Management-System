# Library Management System

A comprehensive web application for managing library operations, built with F# and ASP.NET Core. This system provides user authentication, book catalog management, and book borrowing/returning functionality.

## Features

- **User Authentication**: Secure registration and login with role-based access control
- **Book Management**: Full CRUD operations for books with image upload support
- **Search Functionality**: Case-insensitive search by title or author
- **Book Borrowing**: Users can borrow and return books with tracking
- **Admin Panel**: Administrative functions for book management
- **Responsive UI**: Modern web interface using HTML, CSS, and JavaScript
- **Comprehensive Testing**: Unit and integration tests covering all functionality

## Technologies Used

- **Backend**: F# with ASP.NET Core
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: Cookie-based authentication with BCrypt password hashing
- **Frontend**: Static HTML/CSS/JavaScript with Tailwind CSS
- **Testing**: xUnit with FsUnit
- **Build Tool**: .NET SDK

## Prerequisites

- .NET 10.0 SDK or later
- SQL Server (LocalDB for development)
- Git (for cloning the repository)

## Installation and Setup

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd "Library Management System"
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Update database connection** (optional):
   - Modify `LibraryManagementSystem/appsettings.json` for production database
   - Default configuration uses SQL Server LocalDB

4. **Run database migrations**:
   ```bash
   cd LibraryManagementSystem
   dotnet ef database update
   ```

5. **Build the project**:
   ```bash
   dotnet build
   ```

6. **Run the application**:
   ```bash
   dotnet run --project LibraryManagementSystem
   ```

The application will be available at `https://localhost:7170`

## Usage

### Web Interface

1. **Registration**: Visit `/register.html` to create a new account
2. **Login**: Use `/login.html` to authenticate
3. **Book Management**: Access the main interface at `/index.html`
   - Browse available books
   - Search for books
   - Borrow/return books (authenticated users)
   - Add/edit/delete books (admin users only)

### API Usage

The application provides RESTful API endpoints. See [PROJECT_DESCRIPTION.md](LibraryManagementSystem/PROJECT_DESCRIPTION.md) for complete API documentation.

#### Example API Calls

**Register a new user**:
```bash
curl -X POST https://localhost:7170/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123","rememberMe":false}'
```

**Login**:
```bash
curl -X POST https://localhost:7170/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123","rememberMe":false}'
```

**Get all books** (requires authentication):
```bash
curl -X GET https://localhost:7170/api/books \
  -H "Cookie: .AspNetCore.Cookies=<auth-cookie>"
```

## Project Structure

```
Library Management System/
├── LibraryManagementSystem/          # Main application
│   ├── Controllers/                  # API controllers
│   ├── Services/                     # Business logic services
│   ├── Domain/                       # Models and DTOs
│   │   ├── Models/                   # Entity models
│   │   └── DTOs/                     # Data transfer objects
│   ├── Data/                         # Database context
│   ├── wwwroot/                      # Static web assets
│   ├── PROJECT_DESCRIPTION.md        # Detailed project documentation
│   └── Program.fs                    # Application entry point
├── LibraryManagementSystem.Tests/    # Test project
│   ├── Services/                     # Service unit tests
│   ├── Controllers/                  # Controller integration tests
│   ├── TEST_CASES.md                 # Test documentation
│   └── TestHelpers.fs                # Testing utilities
└── LibraryManagementSystem.sln       # Solution file
```

## Testing

The project includes comprehensive unit and integration tests.

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test project
dotnet test LibraryManagementSystem.Tests/
```

### Test Coverage

- **Unit Tests**: Test individual services with in-memory databases
- **Integration Tests**: Test full API endpoints with SQL LocalDB
- **Test Documentation**: See [TEST_CASES.md](LibraryManagementSystem.Tests/TEST_CASES.md) for detailed test scenarios

## Development

### Code Style

- Follow F# coding conventions
- Use meaningful variable and function names
- Include XML documentation comments
- Handle errors gracefully with Result types

### Database Schema

The application uses Entity Framework Core with the following main entities:
- **Users**: Authentication and authorization
- **Books**: Book catalog with metadata
- **BorrowedBooks**: Borrowing history and status

### Authentication Flow

1. User registers with email/password
2. Password hashed with BCrypt
3. Login validates credentials and sets authentication cookie
4. Cookie contains user claims (ID, email, role)
5. Subsequent requests validated via cookie authentication

## Deployment

### Production Considerations

1. **Database**: Use production SQL Server instance
2. **HTTPS**: Configure SSL certificates
3. **Environment Variables**: Store sensitive data securely
4. **Logging**: Implement structured logging
5. **Security**: Regular security audits and updates

### Guidelines

- Ensure all tests pass
- Follow existing code style
- Update documentation as needed
- Add tests for new functionality

## Support

For questions or issues:
- Check the [PROJECT_DESCRIPTION.md](LibraryManagementSystem/PROJECT_DESCRIPTION.md) for detailed documentation

- Review [TEST_CASES.md](LibraryManagementSystem.Tests/TEST_CASES.md) for testing information
