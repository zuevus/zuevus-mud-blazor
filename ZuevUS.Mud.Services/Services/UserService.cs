using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ZuevUS.Mud.Database;
using ZuevUS.Mud.Services;
using ZuevUS.Mud.Services.Protos;


namespace ZuevUS.Mud.Services.Services;
public class UserService : Users.UsersBase
{
    private readonly IDbContextFactory<DBContext> _contextFactory;
    private readonly ILogger<UserService> _logger;

    public UserService(IDbContextFactory<DBContext> contextFactory, ILogger<UserService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public override async Task<UserResponse> GetUserProfile(GetUserRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var userProfile = await dbContext.UserProfiles.FindAsync(request.UserId);

        return userProfile == null
            ? throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found"))
            : MapToUserResponse(userProfile);
    }

    public override async Task<UserResponse> CreateUserProfile(CreateUserRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var userProfile = new Database.Models.UserProfile
        {
            UserId = request.UserId,
            UserName = request.UserName,
            Email = request.Email,
            Role = (Database.Enum.UserRole)request.Role
        };

        _ = await dbContext.UserProfiles.AddAsync(userProfile);
        _ = await dbContext.SaveChangesAsync();

        _logger.LogInformation("User profile created for user ID: {UserId}", request.UserId);

        return MapToUserResponse(userProfile);
    }

    public override async Task<UserResponse> UpdateUserProfile(UpdateUserRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var userProfile = await dbContext.UserProfiles.FindAsync(request.UserId);

        if (userProfile == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found"));
        }

        userProfile.UserName = request.UserName;
        userProfile.Email = request.Email;
        userProfile.Role = (Database.Enum.UserRole)request.Role;

        _ = dbContext.UserProfiles.Update(userProfile);
        _ = await dbContext.SaveChangesAsync();

        return MapToUserResponse(userProfile);
    }

    public override async Task<DeleteUserResponse> DeleteUserProfile(DeleteUserRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        var userProfile = await dbContext.UserProfiles.FindAsync(request.UserId);

        if (userProfile == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.UserId} not found"));
        }

        _ = dbContext.UserProfiles.Remove(userProfile);
        _ = await dbContext.SaveChangesAsync();

        return new DeleteUserResponse { Success = true };
    }

    public override async Task<UsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        using var dbContext = await _contextFactory.CreateDbContextAsync();

        IQueryable<Database.Models.UserProfile> query = dbContext.UserProfiles;

        if (request.FilterRole != Protos.UserRole.Admin)
        {
            query = query.Where(u => u.Role == (Database.Enum.UserRole)request.FilterRole);
        }

        var users = await query.ToListAsync();

        var response = new UsersResponse();
        response.Users.AddRange(users.Select(MapToUserResponse));

        return response;
    }

    private UserResponse MapToUserResponse(Database.Models.UserProfile userProfile)
    {
        var response = new UserResponse
        {
            UserId = userProfile.UserId,
            UserName = userProfile.UserName,
            Email = userProfile.Email,
            Role = (Protos.UserRole)userProfile.Role,
            CreatedDate = Timestamp.FromDateTime(userProfile.CreatedDate.ToUniversalTime())
        };

        if (userProfile.LastLoginDate.HasValue)
        {
            response.LastLoginDate = Timestamp.FromDateTime(userProfile.LastLoginDate.Value.ToUniversalTime());
        }

        return response;
    }
}
