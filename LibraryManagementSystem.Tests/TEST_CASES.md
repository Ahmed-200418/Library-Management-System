# Library Management System - Test Cases

## Overview

This document outlines the comprehensive test suite for the Library Management System. The tests are organized by component and cover unit tests for services and integration tests for controllers.

## Test Structure

The test suite is divided into:
- **Unit Tests**: Test individual services in isolation using in-memory databases
- **Integration Tests**: Test controller endpoints using a test web application factory with SQL LocalDB

## AuthService Tests

### RegisterAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid Registration | Register with valid email and password | User created successfully with hashed password |
| Empty Email | Attempt registration with empty email | Error: "Email is required" |
| Empty Password | Attempt registration with empty password | Error: "Password is required" |
| Short Password | Password less than 6 characters | Error: "Password must be at least 6 characters" |
| Invalid Email Format | Various invalid email formats | Error: "Invalid email format" |
| Email Trimming | Email with leading/trailing spaces | Email saved trimmed and normalized |
| Case-Insensitive Email | Register with different cases of same email | Second registration fails with "User already exists" |
| Duplicate Email | Register with existing email | Error: "User already exists" |

### ValidateUserAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid Login | Login with correct credentials | User object returned |
| Wrong Password | Login with incorrect password | Error: "Invalid credentials" |
| Empty Email | Login with empty email | Error: "Email is required" |
| Empty Password | Login with empty password | Error: "Password is required" |
| Non-existent Email | Login with email that doesn't exist | Error: "Invalid credentials" |

## BookService Tests

### AddBookAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid Book Addition | Add book with title and author | Book added with auto-generated ID, IsBorrowed = false |
| Empty Title | Add book with empty title | Error: "Book title is required" |
| Empty Author | Add book with empty author | Error: "Book author is required" |
| Force IsBorrowed False | Add book with IsBorrowed = true | IsBorrowed forced to false |

### UpdateBookAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid Update | Update existing book | Book updated successfully |
| ID Mismatch | Update with different ID in URL vs body | Error: "ID mismatch" |
| Empty Title | Update with empty title | Error: "Book title is required" |
| Empty Author | Update with empty author | Error: "Book author is required" |
| Preserve IsBorrowed | Update book without changing IsBorrowed | IsBorrowed field unchanged |

### DeleteBookAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid Deletion | Delete available book | Book deleted successfully |
| Borrowed Book | Delete book that is currently borrowed | Error: "Cannot delete a book that is currently borrowed" |
| Non-existent Book | Delete book with invalid ID | Error: "Book not found" |

### SearchBooksAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Case-Insensitive Search | Search "programming" for "F# Programming" | Book found |
| Empty Query | Search with empty string | All books returned |
| No Matches | Search for non-existent term | Empty list returned |

## BorrowedBooksService Tests

### BorrowBookAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid Borrow | Borrow available book | Borrow record created, book IsBorrowed = true |
| Book Not Found | Borrow non-existent book | Error: "Book not found" |
| Invalid User ID | Borrow with user ID <= 0 | Error: "Invalid user ID" |
| Invalid Book ID | Borrow with book ID <= 0 | Error: "Invalid book ID" |
| Already Borrowed by Another User | Borrow book already borrowed by different user | Error: "Book is already borrowed" |
| Same User Borrows Twice | Same user tries to borrow same book twice | Error: "User has already borrowed this book" |

### ReturnBookAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid Return | Return borrowed book | Borrow record updated with return date, book IsBorrowed = false |
| Wrong User | User tries to return book borrowed by another user | Error: "No active borrow record found" |
| Book Not Found | Return non-existent book | Error: "Book not found" |
| Already Returned | Return book that was already returned | Error: "No active borrow record found" |

### HasUserBorrowedBookAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| User Has Borrowed | Check if user has active borrow for book | Returns true |
| User Has Not Borrowed | Check if user has no borrow record for book | Returns false |
| Invalid User ID | Check with user ID <= 0 | Error: "Invalid user ID" |

### GetUserBorrowedBooksAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Get Active Borrows | Get list of books user has borrowed but not returned | List of active borrow records |
| Invalid User ID | Get borrows for user ID <= 0 | Error: "Invalid user ID" |

### GetBookBorrowHistoryAsync Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Get History | Get all borrow records for a book | List ordered by borrow date descending |
| Book Not Found | Get history for non-existent book | Error: "Book not found" |
| Invalid Book ID | Get history for book ID <= 0 | Error: "Invalid book ID" |

## Controller Integration Tests

### AuthController Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Successful Registration | POST /register with valid data | User created, redirected to /index.html |
| Successful Login | POST /login with valid credentials | Authentication cookie set, returns role |
| Login with Remember Me | POST /login with RememberMe=true | Cookie with 7-day expiration |

### BorrowedBooksController Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Unauthorized Borrow | POST /borrow without authentication | HTTP 401 Unauthorized or 302 Found |

## Test Infrastructure

### Test Helpers

- **In-Memory Database**: Used for unit tests to isolate database operations
- **SQL LocalDB**: Used for integration tests to test full application stack
- **Custom Web Application Factory**: Configures test server with test database
- **Test Data Creation**: Helper functions to create test entities

### Test Organization

- Tests are organized by namespace matching the application structure
- Each test class focuses on a single service or controller
- Test methods use descriptive names indicating the scenario being tested
- Assertions use FsUnit for fluent syntax

### Coverage Areas

- **Happy Path**: Normal successful operations
- **Error Conditions**: Invalid inputs, constraint violations
- **Edge Cases**: Boundary conditions, null values
- **Security**: Authorization requirements, authentication checks
- **Data Integrity**: Database constraints, referential integrity

## Running Tests

Tests can be run using:
```bash
dotnet test
```

Individual test projects:
```bash
dotnet test LibraryManagementSystem.Tests/LibraryManagementSystem.Tests.fsproj
```

## Test Data Management

- Each test creates its own isolated database instance
- Test databases are cleaned up after each test run
- No test data persists between test executions
- In-memory databases for fast unit tests
- SQL LocalDB for realistic integration tests
