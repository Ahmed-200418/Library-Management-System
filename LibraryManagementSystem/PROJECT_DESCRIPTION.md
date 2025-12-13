# Library Management System

## Overview

The Library Management System is a web application built using F# and ASP.NET Core. It provides a comprehensive solution for managing a library's book collection, user authentication, and book borrowing operations. The system supports user registration and login, book CRUD operations, search functionality, and borrowing/returning books.

## Technologies Used

- **Language**: F#
- **Framework**: ASP.NET Core
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: Cookie-based authentication
- **Frontend**: Static HTML/CSS/JavaScript with Tailwind CSS
- **Testing**: xUnit with FsUnit

## Architecture

The application follows a layered architecture:

- **Controllers**: Handle HTTP requests and responses
- **Services**: Contain business logic
- **Domain**: Data models and DTOs
- **Data**: Database context and configuration

## Key Features

- User registration and authentication
- Book management (CRUD operations)
- Book search functionality
- Book borrowing and returning
- Image upload for books
- Role-based authorization (Admin/User)
- Cookie-based session management

## API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|--------------|
| POST | `/api/auth/register` | Register a new user | None |
| POST | `/api/auth/login` | User login | None |
| POST | `/api/auth/logout` | User logout | Required |

### Book Management Endpoints

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|--------------|
| GET | `/api/books` | Get all books | Required |
| GET | `/api/books/{id}` | Get book by ID | Required |
| POST | `/api/books` | Add a new book (with image upload) | Admin |
| PUT | `/api/books/{id}` | Update book | Admin |
| POST | `/api/books/{id}/update-with-image` | Update book with image | Admin |
| DELETE | `/api/books/{id}` | Delete book | Admin |
| GET | `/api/books/search/{query}` | Search books by title or author | Required |

### Book Borrowing Endpoints

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|--------------|
| POST | `/api/books/{id}/borrow` | Borrow a book | Required |
| POST | `/api/books/{id}/return` | Return a book | Required |
| GET | `/api/books/{id}/is-borrowed-by-user` | Check if user has borrowed the book | Required |

## Data Models

### User
- Id: int
- Email: string (unique)
- PasswordHash: string
- Role: string
- CreatedAt: DateTime

### Book
- Id: int
- Title: string
- Author: string
- Description: string
- ImagePath: string
- IsBorrowed: bool
- CreatedAt: DateTime

### BorrowedBook
- Id: int
- UserId: int
- BookId: int
- BorrowDate: DateTime
- ReturnDate: Nullable<DateTime>

## Authentication Flow

1. User registers with email and password
2. Password is hashed using BCrypt
3. User logs in with credentials
4. Upon successful authentication, a cookie is set with user claims (ID, email, role)
5. Subsequent requests include the authentication cookie
6. Admin role is required for book management operations

## Book Management

- Books can be added, updated, and deleted by administrators
- Images can be uploaded for books
- Books can be searched by title or author (case-insensitive)
- Books cannot be deleted if they are currently borrowed
- Borrowing status is tracked automatically

## Security Features

- Password hashing with BCrypt
- Email uniqueness enforcement
- Role-based authorization
- Input validation
- SQL injection prevention through EF Core
