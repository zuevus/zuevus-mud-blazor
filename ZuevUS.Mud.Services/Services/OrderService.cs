using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ZuevUS.Mud.Database;
using ZuevUS.Mud.Database.Models;
using ZuevUS.Mud.Services;
using ZuevUS.Mud.Services.Protos;

namespace ZuevUS.Mud.Services.Services;
public class OrderService : Orders.OrdersBase
{
    private readonly IDbContextFactory<DBContext> _contextFactory;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IDbContextFactory<DBContext> contextFactory, ILogger<OrderService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _logger.LogInformation("test");
    }

    public override async Task<OrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Title))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Title is required"));

        if (string.IsNullOrEmpty(request.ClientName))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Client name is required"));

        if (string.IsNullOrEmpty(request.ClientEmail))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Client email is required"));

        if (request.Deadline == null)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Deadline is required"));

        if (request.Price <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Price must be greater than 0"));

        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var order = new Database.Models.Order
        {
            Title = request.Title,
            Description = request.Description,
            Type = (Database.Enum.OrderType)request.Type,
            Status = Database.Enum.OrderStatus.New,
            Price = (decimal)request.Price,
            Deadline = request.Deadline.ToDateTime(),
            ClientName = request.ClientName,
            ClientEmail = request.ClientEmail,
            CreatedByUserId = request.UserId
        };

        _ = await dbContext.Orders.AddAsync(order);
        _ = await dbContext.SaveChangesAsync();

        _logger.LogInformation("Order created with ID: {OrderId}", order.Id);

        return MapToOrderResponse(order);
    }

    public override async Task<OrdersResponse> GetOrders(GetOrdersRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        IQueryable<Database.Models.Order> query = dbContext.Orders;

        // Filter based on user role
        if (request.UserRole == Protos.UserRole.User)
        {
            query = query.Where(o => o.CreatedByUserId == request.UserId);
        }

        var orders = await query.ToListAsync();

        var response = new OrdersResponse();
        response.Orders.AddRange(orders.Select(MapToOrderResponse));

        return response;
    }

    public override async Task<OrderResponse> GetOrderById(GetOrderByIdRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var order = await dbContext.Orders.FindAsync(request.Id);

        return order == null
            ? throw new RpcException(new Status(StatusCode.NotFound, $"Order with ID {request.Id} not found"))
            : MapToOrderResponse(order);
    }

    public override async Task<OrderResponse> UpdateOrder(UpdateOrderRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var order = await dbContext.Orders.FindAsync(request.Id);

        if (order == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Order with ID {request.Id} not found"));
        }

        order.Title = request.Title;
        order.Description = request.Description;
        order.Type = (Database.Enum.OrderType)request.Type;
        order.Status = (Database.Enum.OrderStatus)request.Status;
        order.Price = (decimal)request.Price;
        order.Deadline = request.Deadline.ToDateTime();

        _ = dbContext.Orders.Update(order);
        _ = await dbContext.SaveChangesAsync();

        return MapToOrderResponse(order);
    }

    public override async Task<DeleteOrderResponse> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var order = await dbContext.Orders.FindAsync(request.Id);

        if (order == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Order with ID {request.Id} not found"));
        }

        _ = dbContext.Orders.Remove(order);
        _ = await dbContext.SaveChangesAsync();

        return new DeleteOrderResponse { Success = true };
    }

    private OrderResponse MapToOrderResponse(Database.Models.Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            Title = order.Title,
            Description = order.Description,
            Type = (Protos.OrderType)order.Type,
            Status = (Protos.OrderStatus)order.Status,
            Price = (double)order.Price,
            CreatedDate = Timestamp.FromDateTime(order.CreatedDate.ToUniversalTime()),
            Deadline = Timestamp.FromDateTime(order.Deadline.ToUniversalTime()),
            ClientName = order.ClientName,
            ClientEmail = order.ClientEmail,
            UserId = order.CreatedByUserId
        };
    }
}