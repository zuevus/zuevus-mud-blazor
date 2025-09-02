using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZuevUS.Mud.Database;
using ZuevUS.Mud.Services.Protos;
using ZuevUS.Mud.Services.Services;

namespace ZuevUS.Mud.Services.Tests;

[TestFixture]
internal class UserServiceTests : TestBase
{
    private UserService _userService;
    private DBContext _dbContext;
    private Mock<ILogger<UserService>> _loggerMock;
    private string _databaseName;

    [SetUp]
    public void Setup()
    {
        _databaseName = Guid.NewGuid().ToString();
        _dbContext = CreateInMemoryContext(_databaseName);

        // Создаем фабрику, которая будет создавать новые контексты с той же БД
        var contextFactory = CreateContextFactory(() => CreateInMemoryContext(_databaseName));
        _loggerMock = CreateLoggerMock<UserService>();

        _userService = new UserService(contextFactory, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public async Task CreateUserProfile_ValidRequest_ReturnsUserResponse()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            UserId = "test-user-id",
            UserName = "Test User",
            Email = "test@user.com",
            Role = UserRole.User
        };

        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _userService.CreateUserProfile(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(request.UserId));
        Assert.That(result.UserName, Is.EqualTo(request.UserName));
        Assert.That(result.Email, Is.EqualTo(request.Email));
        Assert.That(result.Role, Is.EqualTo(request.Role));

        // Verify database - создаем новый контекст для проверки
        using var verificationContext = CreateInMemoryContext(_databaseName);
        var userInDb = await verificationContext.UserProfiles.FindAsync(request.UserId);
        Assert.That(userInDb, Is.Not.Null);
        Assert.That(userInDb.UserName, Is.EqualTo(request.UserName));
    }

    [Test]
    public async Task GetUserProfile_ExistingUser_ReturnsUser()
    {
        // Arrange - используем отдельный контекст для подготовки данных
        using var arrangeContext = CreateInMemoryContext(_databaseName);
        var userProfile = new Database.Models.UserProfile
        {
            UserId = "test-user-id",
            UserName = "Test User",
            Email = "test@user.com",
            Role = Database.Enum.UserRole.User
        };

        await arrangeContext.UserProfiles.AddAsync(userProfile);
        await arrangeContext.SaveChangesAsync();

        var request = new GetUserRequest { UserId = userProfile.UserId };
        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _userService.GetUserProfile(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(userProfile.UserId));
        Assert.That(result.UserName, Is.EqualTo(userProfile.UserName));
    }

    [Test]
    public void GetUserProfile_NonExistingUser_ThrowsNotFoundException()
    {
        // Arrange
        var request = new GetUserRequest { UserId = "non-existing-user" };
        var serverCallContext = CreateTestServerCallContext();

        // Act & Assert
        Assert.ThrowsAsync<RpcException>(async () =>
            await _userService.GetUserProfile(request, serverCallContext));
    }

    [Test]
    public async Task UpdateUserProfile_ValidRequest_UpdatesUser()
    {
        // Arrange
        using var arrangeContext = CreateInMemoryContext(_databaseName);
        var userProfile = new Database.Models.UserProfile
        {
            UserId = "test-user-id",
            UserName = "Original Name",
            Email = "original@user.com",
            Role = Database.Enum.UserRole.User
        };

        await arrangeContext.UserProfiles.AddAsync(userProfile);
        await arrangeContext.SaveChangesAsync();

        var request = new UpdateUserRequest
        {
            UserId = userProfile.UserId,
            UserName = "Updated Name",
            Email = "updated@user.com",
            Role = UserRole.Admin
        };

        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _userService.UpdateUserProfile(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserName, Is.EqualTo("Updated Name"));
        Assert.That(result.Email, Is.EqualTo("updated@user.com"));
        Assert.That(result.Role, Is.EqualTo(UserRole.Admin));

        // Verify database
        using var verificationContext = CreateInMemoryContext(_databaseName);
        var updatedUser = await verificationContext.UserProfiles.FindAsync(userProfile.UserId);
        Assert.That(updatedUser.UserName, Is.EqualTo("Updated Name"));
        Assert.That(updatedUser.Role, Is.EqualTo(Database.Enum.UserRole.Admin));
    }

    [Test]
    public async Task GetUsers_WithFilter_ReturnsFilteredUsers()
    {
        // Arrange
        using var arrangeContext = CreateInMemoryContext(_databaseName);
        var users = new[]
        {
                new Database.Models.UserProfile
                {
                    UserId = "user1",
                    UserName = "User 1",
                    Email = "user1@test.com",
                    Role = Database.Enum.UserRole.User
                },
                new Database.Models.UserProfile
                {
                    UserId = "admin1",
                    UserName = "Admin 1",
                    Email = "admin1@test.com",
                    Role = Database.Enum.UserRole.Admin
                }
            };

        await arrangeContext.UserProfiles.AddRangeAsync(users);
        await arrangeContext.SaveChangesAsync();

        var request = new GetUsersRequest { FilterRole = UserRole.User };
        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _userService.GetUsers(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Users, Has.Count.EqualTo(1));
        Assert.That(result.Users[0].Role, Is.EqualTo(UserRole.User));
    }
}