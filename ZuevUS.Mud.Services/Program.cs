using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ZuevUS.Mud.Database;
using ZuevUS.Mud.Services.Services;

var builder = WebApplication.CreateBuilder(args);
var sr = builder.Configuration.GetSection("Serilog").GetSection("Properties").GetValue(typeof(string), "Application");
Console.WriteLine(sr);
Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine(builder.Configuration.GetValue(typeof(string), "Something"));

var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Logging.AddSerilog(loggerConfiguration);

// Add services to the container.
builder.Services.AddGrpc();


builder.Services.AddDbContextFactory<DBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));



var app = builder.Build();

//app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
//app.MapGrpcService<OrderService>();
app.MapGrpcService<OrderService>();
app.MapGrpcService<UserService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
