using LastHour.BuildingBlocks.Infrastructure.Persistence;
using LastHour.BuildingBlocks.Infrastructure.Persistence.UnitOfWork;
using LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Persistence;

public class EfUnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_CommitsPendingChanges()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var unitOfWork = new EfUnitOfWork(context);
        context.Set<TestAggregate>().Add(new TestAggregate("merchant"));

        int written = await unitOfWork.SaveChangesAsync();

        Assert.Equal(1, written);
        Assert.Equal(1, await context.Set<TestAggregate>().CountAsync());
    }

    [Fact]
    public async Task CommitTransactionAsync_WithoutStartedTransaction_Throws()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var unitOfWork = new EfUnitOfWork(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.CommitTransactionAsync());
    }

    [Fact]
    public async Task RollbackTransactionAsync_WithoutStartedTransaction_Throws()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var unitOfWork = new EfUnitOfWork(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.RollbackTransactionAsync());
    }
}
