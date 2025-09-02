using Google.Protobuf.WellKnownTypes;
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
internal class OrderServiceTests : TestBase
{
    private OrderService _orderService;
    private DBContext _dbContext;
    private Mock<ILogger<OrderService>> _loggerMock;
    private string _databaseName;

    [SetUp]
    public void Setup()
    {
        _databaseName = Guid.NewGuid().ToString();
        _dbContext = CreateInMemoryContext(_databaseName);

        // Создаем фабрику, которая будет создавать новые контексты с той же БД
        var contextFactory = CreateContextFactory(() => CreateInMemoryContext(_databaseName));
        _loggerMock = CreateLoggerMock<OrderService>();

        _orderService = new OrderService(contextFactory, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public async Task CreateOrder_ValidRequest_ReturnsOrderResponse()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Title = "Test Order",
            Description = "Test Description",
            Type = OrderType.WebsiteDevelopment,
            Price = 1000.0,
            Deadline = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(7).ToUniversalTime()),
            ClientName = "Test Client",
            ClientEmail = "test@client.com",
            UserId = "test-user-id"
        };

        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _orderService.CreateOrder(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(request.Title));
        Assert.That(result.Description, Is.EqualTo(request.Description));
        Assert.That(result.Price, Is.EqualTo(request.Price));
        Assert.That(result.ClientName, Is.EqualTo(request.ClientName));

        // Verify database - создаем новый контекст для проверки
        using var verificationContext = CreateInMemoryContext(_databaseName);
        var orderInDb = await verificationContext.Orders.FirstOrDefaultAsync();
        Assert.That(orderInDb, Is.Not.Null);
        Assert.That(orderInDb.Title, Is.EqualTo(request.Title));
    }

    [Test]
    public void CreateOrder_InvalidDeadline_ThrowsRpcException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Title = "Test Order",
            Deadline = null, // Invalid deadline
            ClientName = "Test Client",
            ClientEmail = "test@client.com",
            UserId = "test-user-id"
        };

        var serverCallContext = CreateTestServerCallContext();

        // Act & Assert
        Assert.ThrowsAsync<RpcException>(async () =>
            await _orderService.CreateOrder(request, serverCallContext));
    }

    [Test]
    public async Task GetOrderById_ExistingOrder_ReturnsOrder()
    {
        // Arrange - используем отдельный контекст для подготовки данных
        using var arrangeContext = CreateInMemoryContext(_databaseName);
        var order = new Database.Models.Order
        {
            Title = "Test Order",
            Description = "Test Description",
            Type = Database.Enum.OrderType.WebsiteDevelopment,
            Status = Database.Enum.OrderStatus.New,
            Price = 1000.0m,
            Deadline = DateTime.UtcNow.AddDays(7),
            ClientName = "Test Client",
            ClientEmail = "test@client.com",
            CreatedByUserId = "test-user-id"
        };

        await arrangeContext.Orders.AddAsync(order);
        await arrangeContext.SaveChangesAsync();

        var request = new GetOrderByIdRequest { Id = order.Id };
        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _orderService.GetOrderById(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(order.Id));
        Assert.That(result.Title, Is.EqualTo(order.Title));
    }

    [Test]
    public void GetOrderById_NonExistingOrder_ThrowsNotFoundException()
    {
        // Arrange
        var request = new GetOrderByIdRequest { Id = 999 };
        var serverCallContext = CreateTestServerCallContext();

        // Act & Assert
        Assert.ThrowsAsync<RpcException>(async () =>
            await _orderService.GetOrderById(request, serverCallContext));
    }

    [Test]
    public async Task GetOrders_AdminUser_ReturnsAllOrders()
    {
        // Arrange - используем отдельный контекст для подготовки данных
        using var arrangeContext = CreateInMemoryContext(_databaseName);
        var orders = new[]
        {
            new Database.Models.Order
            {
                Title = "Order 1",
                CreatedByUserId = "user1",
                ClientName = "Client 1",
                ClientEmail = "client1@test.com",
                Deadline = DateTime.UtcNow.AddDays(1),
                Price = 100m
            },
            new Database.Models.Order
            {
                Title = "Order 2",
                CreatedByUserId = "user2",
                ClientName = "Client 2",
                ClientEmail = "client2@test.com",
                Deadline = DateTime.UtcNow.AddDays(2),
                Price = 200m
            }
        };

        await arrangeContext.Orders.AddRangeAsync(orders);
        await arrangeContext.SaveChangesAsync();

        var request = new GetOrdersRequest
        {
            UserId = "admin-user",
            UserRole = UserRole.Admin
        };

        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _orderService.GetOrders(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Orders, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetOrders_RegularUser_ReturnsOnlyUserOrders()
    {
        // Arrange - используем отдельный контекст для подготовки данных
        using var arrangeContext = CreateInMemoryContext(_databaseName);
        var userId = "test-user";

        var orders = new[]
        {
            new Database.Models.Order
            {
                Title = "User Order",
                CreatedByUserId = userId,
                ClientName = "Client 1",
                ClientEmail = "client1@test.com",
                Deadline = DateTime.UtcNow.AddDays(1),
                Price = 100m
            },
            new Database.Models.Order
            {
                Title = "Other User Order",
                CreatedByUserId = "other-user",
                ClientName = "Client 2",
                ClientEmail = "client2@test.com",
                Deadline = DateTime.UtcNow.AddDays(2),
                Price = 200m
            }
        };

        await arrangeContext.Orders.AddRangeAsync(orders);
        await arrangeContext.SaveChangesAsync();

        var request = new GetOrdersRequest
        {
            UserId = userId,
            UserRole = UserRole.User
        };

        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _orderService.GetOrders(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Orders, Has.Count.EqualTo(1));
        Assert.That(result.Orders[0].Title, Is.EqualTo("User Order"));
    }

    [Test]
    public async Task UpdateOrder_ValidRequest_UpdatesOrder()
    {
        // Arrange - используем отдельный контекст для подготовки данных
        using var arrangeContext = CreateInMemoryContext(_databaseName);
        var order = new Database.Models.Order
        {
            Title = "Original Title",
            Description = "Original Description",
            Type = Database.Enum.OrderType.WebsiteDevelopment,
            Status = Database.Enum.OrderStatus.New,
            Price = 1000.0m,
            Deadline = DateTime.UtcNow.AddDays(7),
            ClientName = "Test Client",
            ClientEmail = "test@client.com",
            CreatedByUserId = "test-user-id"
        };

        await arrangeContext.Orders.AddAsync(order);
        await arrangeContext.SaveChangesAsync();

        var request = new UpdateOrderRequest
        {
            Id = order.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            Type = OrderType.MobileApp,
            Status = OrderStatus.InProgress,
            Price = 1500.0,
            Deadline = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(14).ToUniversalTime())
        };

        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _orderService.UpdateOrder(request, serverCallContext);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Updated Title"));
        Assert.That(result.Description, Is.EqualTo("Updated Description"));
        Assert.That(result.Type, Is.EqualTo(OrderType.MobileApp));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.InProgress));
        Assert.That(result.Price, Is.EqualTo(1500.0));

        // Verify database - создаем новый контекст для проверки
        using var verificationContext = CreateInMemoryContext(_databaseName);
        var updatedOrder = await verificationContext.Orders.FindAsync(order.Id);
        Assert.That(updatedOrder.Title, Is.EqualTo("Updated Title"));
    }

    [Test]
    public async Task DeleteOrder_ExistingOrder_ReturnsSuccess()
    {
        // Arrange - используем отдельный контекст для подготовки данных
        using var arrangeContext = CreateInMemoryContext(_databaseName);
        var order = new Database.Models.Order
        {
            Title = "Order to delete",
            ClientName = "Test Client",
            ClientEmail = "test@client.com",
            Deadline = DateTime.UtcNow.AddDays(7),
            Price = 1000.0m,
            CreatedByUserId = "test-user-id"
        };

        await arrangeContext.Orders.AddAsync(order);
        await arrangeContext.SaveChangesAsync();

        var request = new DeleteOrderRequest { Id = order.Id };
        var serverCallContext = CreateTestServerCallContext();

        // Act
        var result = await _orderService.DeleteOrder(request, serverCallContext);

        // Assert
        Assert.That(result.Success, Is.True);

        // Verify order is deleted - создаем новый контекст для проверки
        using var verificationContext = CreateInMemoryContext(_databaseName);
        var deletedOrder = await verificationContext.Orders.FindAsync(order.Id);
        Assert.That(deletedOrder, Is.Null);
    }

    [Test]
    public async Task UpdateOrder_NonExistingOrder_ThrowsNotFoundException()
    {
        // Arrange
        var request = new UpdateOrderRequest
        {
            Id = 999, // Non-existing ID
            Title = "Updated Title",
            Type = OrderType.MobileApp,
            Status = OrderStatus.InProgress,
            Price = 1500.0,
            Deadline = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(14).ToUniversalTime())
        };

        var serverCallContext = CreateTestServerCallContext();

        // Act & Assert
        Assert.ThrowsAsync<RpcException>(async () =>
            await _orderService.UpdateOrder(request, serverCallContext));
    }

    [Test]
    public async Task DeleteOrder_NonExistingOrder_ThrowsNotFoundException()
    {
        // Arrange
        var request = new DeleteOrderRequest { Id = 999 }; // Non-existing ID
        var serverCallContext = CreateTestServerCallContext();

        // Act & Assert
        Assert.ThrowsAsync<RpcException>(async () =>
            await _orderService.DeleteOrder(request, serverCallContext));
    }
}