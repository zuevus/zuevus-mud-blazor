using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Grpc.Core;
using Grpc.Core.Testing;
using ZuevUS.Mud.Database;

namespace ZuevUS.Mud.Services.Tests;

internal abstract class TestBase
{
    protected DBContext CreateInMemoryContext(string databaseName = null)
    {
        var options = new DbContextOptionsBuilder<DBContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new DBContext(options);
    }

    protected IDbContextFactory<DBContext> CreateContextFactory(Func<DBContext> contextFactory)
    {
        var mockFactory = new Mock<IDbContextFactory<DBContext>>();
        mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contextFactory);
        mockFactory.Setup(f => f.CreateDbContext())
            .Returns(contextFactory);

        return mockFactory.Object;
    }

    protected Mock<ILogger<T>> CreateLoggerMock<T>() where T : class
    {
        return new Mock<ILogger<T>>();
    }

    protected ServerCallContext CreateTestServerCallContext()
    {
        return TestServerCallContext.Create(
            method: "TestMethod",
            host: "localhost",
            deadline: DateTime.UtcNow.AddMinutes(5),
            requestHeaders: new Metadata(),
            cancellationToken: CancellationToken.None,
            peer: "127.0.0.1:5000",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => new WriteOptions(),
            writeOptionsSetter: _ => { }
        );
    }

    // Метод для создания контекста с той же базой данных
    protected DBContext CreateContextFromSameDatabase(DBContext originalContext)
    {
        var databaseName = originalContext.Database.GetDbConnection().Database;
        var options = new DbContextOptionsBuilder<DBContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new DBContext(options);
    }
}

