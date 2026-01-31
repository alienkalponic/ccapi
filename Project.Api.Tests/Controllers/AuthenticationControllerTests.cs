using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Project.Api.Controllers;
using Project.Application.Common.Repository;
using Project.Domain.Utility;
using System.Data;

namespace Project.Api.Tests.Controllers
{
    /// <summary>
    /// Unit tests for AuthenticationController Login API
    /// </summary>
    public class AuthenticationControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly LogService _logService;
        private readonly Mock<IApiResponseService> _mockResponseService;
        private readonly AuthenticationController _controller;

        public AuthenticationControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<LogService>>();
            _logService = new LogService(mockLogger.Object);
            _mockResponseService = new Mock<IApiResponseService>();

            _controller = new AuthenticationController(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _logService,
                _mockResponseService.Object
            );
        }

        #region Successful Login Tests

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccessWithToken()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "testuser@example.com",
                Password = "TestPassword123"
            };

            var loginJsonResponse = @"{
                ""Status"": true,
                ""ContactId"": ""1"",
                ""FirstName"": ""John"",
                ""LastName"": ""Doe"",
                ""RoleId"": ""2"",
                ""RoleName"": ""Admin"",
                ""UserEmail"": ""testuser@example.com""
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            var expectedResponse = new ApiResponse
            {
                Success = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Response = new LoginResponseDto
                {
                    UserDetails = new LoginUserDetails
                    {
                        ContactId = 1,
                        FirstName = "John",
                        LastName = "Doe",
                        RoleId = 2,
                        RoleName = "Admin"
                    },
                    AccessToken = "mock-jwt-token"
                }
            };

            _mockResponseService
                .Setup(x => x.Success(It.IsAny<LoginResponseDto>(), It.IsAny<System.Net.HttpStatusCode>()))
                .Returns(new ActionResult<ApiResponse>(expectedResponse));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            Assert.NotNull(actionResult.Value);
            Assert.True(actionResult.Value.Success);
            Assert.Equal(System.Net.HttpStatusCode.OK, actionResult.Value.StatusCode);
            
            var loginResponse = Assert.IsType<LoginResponseDto>(actionResult.Value.Response);
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotNull(loginResponse.UserDetails);
            Assert.Equal("John", loginResponse.UserDetails.FirstName);
            Assert.Equal("Doe", loginResponse.UserDetails.LastName);
        }

        [Fact]
        public async Task Login_WithValidCredentials_CallsStoredProcedureWithCorrectParameters()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "admin@example.com",
                Password = "AdminPass123"
            };

            var loginJsonResponse = @"{
                ""Status"": true,
                ""ContactId"": ""5"",
                ""FirstName"": ""Admin"",
                ""LastName"": ""User"",
                ""RoleId"": ""1"",
                ""RoleName"": ""SuperAdmin"",
                ""UserEmail"": ""admin@example.com""
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    "Sp_Circle_LOGIN_ACTIVITY",
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            _mockResponseService
                .Setup(x => x.Success(It.IsAny<LoginResponseDto>(), It.IsAny<System.Net.HttpStatusCode>()))
                .Returns(new ActionResult<ApiResponse>(new ApiResponse { Success = true }));

            // Act
            await _controller.Login(loginRequest);

            // Assert
            _mockUnitOfWork.Verify(
                x => x.contactRepository.CallStoreProcedure(
                    "Sp_Circle_LOGIN_ACTIVITY",
                    It.Is<SqlParameter[]>(p => 
                        p.Length == 3 &&
                        p[0].ParameterName == "@OPERATION_ID" &&
                        p[0].Value.Equals(1) &&
                        p[1].ParameterName == "@EMAIL_ID" &&
                        p[1].Value.Equals(loginRequest.UserName) &&
                        p[2].ParameterName == "@PASSWORD"
                    )),
                Times.Once);
        }

        [Fact]
        public async Task Login_WithValidCredentials_EncryptsPassword()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "user@test.com",
                Password = "PlainPassword"
            };

            var loginJsonResponse = @"{
                ""Status"": true,
                ""ContactId"": ""10"",
                ""FirstName"": ""Test"",
                ""LastName"": ""User"",
                ""RoleId"": ""3"",
                ""RoleName"": ""User"",
                ""UserEmail"": ""user@test.com""
            }";

            string capturedPassword = null;

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .Callback<string, SqlParameter[]>((sp, parameters) =>
                {
                    capturedPassword = parameters[2].Value?.ToString();
                })
                .ReturnsAsync(loginJsonResponse);

            _mockResponseService
                .Setup(x => x.Success(It.IsAny<LoginResponseDto>(), It.IsAny<System.Net.HttpStatusCode>()))
                .Returns(new ActionResult<ApiResponse>(new ApiResponse { Success = true }));

            // Act
            await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(capturedPassword);
            Assert.NotEqual("PlainPassword", capturedPassword); // Password should be encrypted
        }

        #endregion

        #region Failed Login Tests

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "invalid@example.com",
                Password = "WrongPassword"
            };

            var loginJsonResponse = @"{
                ""Status"": false,
                ""Message"": ""Invalid credentials""
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            _mockMapper
                .Setup(x => x.Map<LoginUserDetails>(It.IsAny<string>()))
                .Returns((LoginUserDetails)null);

            var expectedResponse = new ApiResponse
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ErrorMassage = new List<string> { "Email or Password is incorrect" }
            };

            _mockResponseService
                .Setup(x => x.BadRequest(It.IsAny<string>()))
                .Returns(new ActionResult<ApiResponse>(expectedResponse));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            Assert.NotNull(actionResult.Value);
            Assert.False(actionResult.Value.Success);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, actionResult.Value.StatusCode);
            Assert.Contains("Email or Password is incorrect", actionResult.Value.ErrorMassage);
        }

        [Fact]
        public async Task Login_WithNullUsername_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = null,
                Password = "Password123"
            };

            var loginJsonResponse = @"{
                ""Status"": false
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            _mockMapper
                .Setup(x => x.Map<LoginUserDetails>(It.IsAny<string>()))
                .Returns((LoginUserDetails)null);

            var expectedResponse = new ApiResponse
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ErrorMassage = new List<string> { "Email or Password is incorrect" }
            };

            _mockResponseService
                .Setup(x => x.BadRequest(It.IsAny<string>()))
                .Returns(new ActionResult<ApiResponse>(expectedResponse));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            Assert.False(actionResult.Value.Success);
        }

        [Fact]
        public async Task Login_WithNullPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "user@example.com",
                Password = null
            };

            var loginJsonResponse = @"{
                ""Status"": false
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            _mockMapper
                .Setup(x => x.Map<LoginUserDetails>(It.IsAny<string>()))
                .Returns((LoginUserDetails)null);

            var expectedResponse = new ApiResponse
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ErrorMassage = new List<string> { "Email or Password is incorrect" }
            };

            _mockResponseService
                .Setup( x => x.BadRequest(It.IsAny<string>()))
                .Returns(new ActionResult<ApiResponse>(expectedResponse));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            Assert.False(actionResult.Value.Success);
        }

        [Fact]
        public async Task Login_WithEmptyCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "",
                Password = ""
            };

            var loginJsonResponse = @"{
                ""Status"": false
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            _mockMapper
                .Setup(x => x.Map<LoginUserDetails>(It.IsAny<string>()))
                .Returns((LoginUserDetails)null);

            var expectedResponse = new ApiResponse
            {
                Success = false,
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ErrorMassage = new List<string> { "Email or Password is incorrect" }
            };

            _mockResponseService
                .Setup(x => x.BadRequest(It.IsAny<string>()))
                .Returns(new ActionResult<ApiResponse>(expectedResponse));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            Assert.False(actionResult.Value.Success);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, actionResult.Value.StatusCode);
        }

        #endregion

        #region Edge Case Tests

         [Fact]
        public async Task Login_WithDatabaseException_ThrowsException()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "user@example.com",
                Password = "Password123"
            };

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                async () => await _controller.Login(loginRequest));
        }

        [Fact]
        public async Task Login_WithSpecialCharactersInUsername_ProcessesCorrectly()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "user+test@example.com",
                Password = "Password123"
            };

            var loginJsonResponse = @"{
                ""Status"": true,
                ""ContactId"": ""15"",
                ""FirstName"": ""Special"",
                ""LastName"": ""User"",
                ""RoleId"": ""2"",
                ""RoleName"": ""User"",
                ""UserEmail"": ""user+test@example.com""
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            _mockResponseService
                .Setup(x => x.Success(It.IsAny<LoginResponseDto>(), It.IsAny<System.Net.HttpStatusCode>()))
                .Returns(new ActionResult<ApiResponse>(new ApiResponse { Success = true }));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            _mockUnitOfWork.Verify(
                x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.Is<SqlParameter[]>(p => p[1].Value.Equals("user+test@example.com"))),
                Times.Once);
        }

        [Fact]
        public async Task Login_WithMultipleRoles_ReturnsCorrectRoleInformation()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "admin@example.com",
                Password = "AdminPass123"
            };

            var loginJsonResponse = @"{
                ""Status"": true,
                ""ContactId"": ""1"",
                ""FirstName"": ""Super"",
                ""LastName"": ""Admin"",
                ""RoleId"": ""1"",
                ""RoleName"": ""SuperAdmin"",
                ""UserEmail"": ""admin@example.com""
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            var expectedResponse = new ApiResponse
            {
                Success = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Response = new LoginResponseDto
                {
                    UserDetails = new LoginUserDetails
                    {
                        ContactId = 1,
                        RoleId = 1,
                        RoleName = "SuperAdmin"
                    },
                    AccessToken = "mock-token"
                }
            };

            _mockResponseService
                .Setup(x => x.Success(It.IsAny<LoginResponseDto>(), It.IsAny<System.Net.HttpStatusCode>()))
                .Returns(new ActionResult<ApiResponse>(expectedResponse));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            var loginResponse = Assert.IsType<LoginResponseDto>(actionResult.Value.Response);
            Assert.Equal(1, loginResponse.UserDetails.RoleId);
            Assert.Equal("SuperAdmin", loginResponse.UserDetails.RoleName);
        }

        [Theory]
        [InlineData("user@test.com", "Pass123")]
        [InlineData("admin@company.com", "AdminPassword")]
        [InlineData("test.user@example.co.uk", "TestPass456")]
        public async Task Login_WithVariousValidCredentials_ReturnsSuccess(string username, string password)
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = username,
                Password = password
            };

            var loginJsonResponse = $@"{{
                ""Status"": true,
                ""ContactId"": ""1"",
                ""FirstName"": ""Test"",
                ""LastName"": ""User"",
                ""RoleId"": ""2"",
                ""RoleName"": ""User"",
                ""UserEmail"": ""{username}""
            }}";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            _mockResponseService
                .Setup(x => x.Success(It.IsAny<LoginResponseDto>(), It.IsAny<System.Net.HttpStatusCode>()))
                .Returns(new ActionResult<ApiResponse>(new ApiResponse { Success = true }));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            Assert.True(actionResult.Value.Success);
        }

        #endregion

        #region JWT Token Tests

        [Fact]
        public async Task Login_WithValidCredentials_GeneratesJWTToken()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "user@example.com",
                Password = "Password123"
            };

            var loginJsonResponse = @"{
                ""Status"": true,
                ""ContactId"": ""1"",
                ""FirstName"": ""John"",
                ""LastName"": ""Doe"",
                ""RoleId"": ""2"",
                ""RoleName"": ""User"",
                ""UserEmail"": ""user@example.com""
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            var expectedResponse = new ApiResponse
            {
                Success = true,
                Response = new LoginResponseDto
                {
                    AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock.token",
                    UserDetails = new LoginUserDetails { ContactId = 1 }
                }
            };

            _mockResponseService
                .Setup(x => x.Success(It.IsAny<LoginResponseDto>(), It.IsAny<System.Net.HttpStatusCode>()))
                .Returns(new ActionResult<ApiResponse>(expectedResponse));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            var loginResponse = Assert.IsType<LoginResponseDto>(actionResult.Value.Response);
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotEmpty(loginResponse.AccessToken);
        }

        [Fact]
        public async Task Login_SuccessfulLogin_TokenContainsUserClaims()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                UserName = "claims@test.com",
                Password = "Password123"
            };

            var loginJsonResponse = @"{
                ""Status"": true,
                ""ContactId"": ""100"",
                ""FirstName"": ""Claims"",
                ""LastName"": ""Test"",
                ""RoleId"": ""5"",
                ""RoleName"": ""Manager"",
                ""UserEmail"": ""claims@test.com""
            }";

            _mockUnitOfWork
                .Setup(x => x.contactRepository.CallStoreProcedure(
                    It.IsAny<string>(),
                    It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(loginJsonResponse);

            var expectedResponse = new ApiResponse
            {
                Success = true,
                Response = new LoginResponseDto
                {
                    AccessToken = "mock-jwt-with-claims",
                    UserDetails = new LoginUserDetails
                    {
                        ContactId = 100,
                        FirstName = "Claims",
                        LastName = "Test",
                        RoleId = 5,
                        RoleName = "Manager"
                    }
                }
            };

            _mockResponseService
                .Setup(x => x.Success(It.IsAny<LoginResponseDto>(), It.IsAny<System.Net.HttpStatusCode>()))
                .Returns(new ActionResult<ApiResponse>(expectedResponse));

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            var actionResult = Assert.IsType<ActionResult<ApiResponse>>(result);
            var loginResponse = Assert.IsType<LoginResponseDto>(actionResult.Value.Response);
            
            // Verify that user details are populated (which would be in JWT claims)
            Assert.Equal(100, loginResponse.UserDetails.ContactId);
            Assert.Equal("Claims", loginResponse.UserDetails.FirstName);
            Assert.Equal("Test", loginResponse.UserDetails.LastName);
            Assert.Equal(5, loginResponse.UserDetails.RoleId);
            Assert.Equal("Manager", loginResponse.UserDetails.RoleName);
        }

        #endregion
    }
}
