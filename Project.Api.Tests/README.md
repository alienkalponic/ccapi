# Project.Api.Tests - xUnit Test Suite

This project contains comprehensive unit tests for the Project.Api, specifically focusing on the **Authentication API** (Login functionality).

## 📋 Overview

The test suite uses **xUnit** as the testing framework along with **Moq** for mocking dependencies. It provides comprehensive coverage for the Login API endpoint including successful scenarios, failure cases, edge cases, and JWT token generation.

## 🧪 Test Coverage

### AuthenticationControllerTests

The `AuthenticationControllerTests` class contains 20+ unit tests covering the following scenarios:

#### ✅ Successful Login Tests
- **Login_WithValidCredentials_ReturnsSuccessWithToken**: Verifies successful login with valid credentials returns JWT token
- **Login_WithValidCredentials_CallsStoredProcedureWithCorrectParameters**: Ensures stored procedure is called with correct parameters
- **Login_WithValidCredentials_EncryptsPassword**: Validates password encryption before database call
- **Login_WithVariousValidCredentials_ReturnsSuccess**: Theory-based test with multiple valid credential combinations

#### ❌ Failed Login Tests
- **Login_WithInvalidCredentials_ReturnsBadRequest**: Tests login failure with wrong credentials
- **Login_WithNullUsername_ReturnsBadRequest**: Validates handling of null username
- **Login_WithNullPassword_ReturnsBadRequest**: Validates handling of null password
- **Login_WithEmptyCredentials_ReturnsBadRequest**: Tests empty string credentials

#### 🔍 Edge Case Tests
- **Login_WithDatabaseException_ThrowsException**: Ensures database exceptions are properly thrown
- **Login_WithSpecialCharactersInUsername_ProcessesCorrectly**: Tests special characters in email (e.g., `user+test@example.com`)
- **Login_WithMultipleRoles_ReturnsCorrectRoleInformation**: Validates role information handling

#### 🔐 JWT Token Tests
- **Login_WithValidCredentials_GeneratesJWTToken**: Verifies JWT token generation
- **Login_SuccessfulLogin_TokenContainsUserClaims**: Ensures token contains correct user claims (name, role, email, ID)

## 🏗️ Test Structure

### Dependencies Mocked
- `IUnitOfWork`: Database operations
- `IMapper`: AutoMapper for object mapping
- `LogService`: Logging service
- `IApiResponseService`: API response formatting

### Test Anatomy
Each test follows the **AAA (Arrange-Act-Assert)** pattern:

```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsSuccessWithToken()
{
    // Arrange - Set up test data and mocks
    var loginRequest = new LoginRequestDTO { ... };
    _mockUnitOfWork.Setup(...);
    
    // Act - Execute the method under test
    var result = await _controller.Login(loginRequest);
    
    // Assert - Verify the results
    Assert.NotNull(result);
    Assert.True(result.Value.IsSuccess);
}
```

## 🚀 Running the Tests

### Using Visual Studio
1. Open the solution in Visual Studio
2. Go to **Test** → **Test Explorer**
3. Click **Run All Tests**

### Using .NET CLI
```bash
# Navigate to the test project directory
cd "Project.Api.Tests"

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity detailed

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Using Visual Studio Test Explorer
- **Run All**: Ctrl + R, A
- **Run Last Tests**: Ctrl + R, L
- **Debug All Tests**: Ctrl + R, Ctrl + A

## 📦 NuGet Packages Used

| Package | Version | Purpose |
|---------|---------|---------|
| xunit | 2.6.2 | Testing framework |
| xunit.runner.visualstudio | 2.5.4 | Visual Studio test runner |
| Moq | 4.20.70 | Mocking framework |
| Microsoft.NET.Test.Sdk | 17.8.0 | .NET test platform |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | ASP.NET Core integration testing |
| AutoMapper | 13.0.1 | Object mapping |
| coverlet.collector | 6.0.0 | Code coverage collection |

## 📊 Test Results Example

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    20, Skipped:     0, Total:    20, Duration: 250 ms
```

## 🔧 Extending the Tests

To add new tests:

1. Add a new test method with `[Fact]` attribute:
```csharp
[Fact]
public async Task YourTestName()
{
    // Arrange
    // Act
    // Assert
}
```

2. For parameterized tests, use `[Theory]` and `[InlineData]`:
```csharp
[Theory]
[InlineData("user1@test.com", "Pass1")]
[InlineData("user2@test.com", "Pass2")]
public async Task YourParameterizedTest(string username, string password)
{
    // Test logic
}
```

## 🎯 Best Practices Implemented

✔️ **Isolation**: Each test is independent and doesn't affect others  
✔️ **Descriptive Names**: Test names clearly describe what they test  
✔️ **AAA Pattern**: Consistent Arrange-Act-Assert structure  
✔️ **Comprehensive Coverage**: Multiple scenarios including edge cases  
✔️ **Fast Execution**: All tests use mocks, no database calls  
✔️ **Maintainability**: Clear and well-organized test structure  

## 📝 Notes

- All tests use mocked dependencies to ensure fast execution and isolation
- Password encryption is tested by verifying the password is transformed before database call
- JWT token generation is implicitly tested through the controller's response
- The actual stored procedure (`Sp_Circle_LOGIN_ACTIVITY`) is not called; it's mocked

## 🔐 Security Testing

The test suite validates:
- ✅ Password encryption before storage
- ✅ Proper handling of authentication failures
- ✅ JWT token generation with user claims
- ✅ Input validation (null, empty credentials)

## 🐛 Troubleshooting

### Tests not appearing in Test Explorer
- Clean and rebuild the solution
- Close and reopen Visual Studio
- Ensure the test project is built successfully

### Mock setup not working
- Verify the mock setup matches the actual method signature
- Check that `It.IsAny<T>()` is used correctly
- Ensure async methods use `ReturnsAsync` instead of `Returns`

### Build errors
- Ensure all NuGet packages are restored: `dotnet restore`
- Check that project references are correct
- Verify .NET 8.0 SDK is installed

## 📚 Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [ASP.NET Core Testing](https://docs.microsoft.com/aspnet/core/test)

---

**Created**: December 2025  
**Framework**: .NET 8.0  
**Test Framework**: xUnit v2.6.2  
**Mocking Framework**: Moq v4.20.70
