using LastHour.BuildingBlocks.Infrastructure.Persistence;
using LastHour.BuildingBlocks.Infrastructure.Persistence.Repositories;
using LastHour.BuildingBlocks.Infrastructure.Persistence.UnitOfWork;
using LastHour.BuildingBlocks.Infrastructure.Tests.Persistence.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Persistence;

public class EfRepositoryTests
{
    [Fact]
    public async Task Add_ThenCommit_PersistsAggregate()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var repository = new EfRepository<TestAggregate>(context);
        var unitOfWork = new EfUnitOfWork(context);
        var aggregate = new TestAggregate("merchant");

        repository.Add(aggregate);
        int saved = await unitOfWork.SaveChangesAsync();

        Assert.Equal(1, saved);
        Assert.True(await context.Set<TestAggregate>().AnyAsync());
    }

    [Fact]
    public async Task Update_ThenCommit_PersistsMutation()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var repository = new EfRepository<TestAggregate>(context);
        var unitOfWork = new EfUnitOfWork(context);
        var aggregate = new TestAggregate("merchant");
        repository.Add(aggregate);
        await unitOfWork.SaveChangesAsync();

        aggregate.Rename("renamed");
        repository.Update(aggregate);
        await unitOfWork.SaveChangesAsync();

        TestAggregate stored = await context.Set<TestAggregate>().AsNoTracking().SingleAsync();
        Assert.Equal("renamed", stored.Name);
    }

    [Fact]
    public async Task Remove_ThenCommit_SoftDeletesAggregate()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var repository = new EfRepository<TestAggregate>(context);
        var unitOfWork = new EfUnitOfWork(context);
        var aggregate = new TestAggregate("merchant");
        repository.Add(aggregate);
        await unitOfWork.SaveChangesAsync();

        repository.Remove(aggregate);
        await unitOfWork.SaveChangesAsync();

        TestAggregate stored = await context.Set<TestAggregate>().IgnoreQueryFilters().SingleAsync();
        Assert.True(stored.IsDeleted);
    }

    [Fact]
    public async Task RemoveRange_ThenCommit_RemovesAllAggregates()
    {
        using LastHourDbContext context = TestPersistence.CreateContext();
        var repository = new EfRepository<TestAggregate>(context);
        var unitOfWork = new EfUnitOfWork(context);
        var first = new TestAggregate("first");
        var second = new TestAggregate("second");
        repository.Add(first);
        repository.Add(second);
        await unitOfWork.SaveChangesAsync();

        repository.RemoveRange(new[] { first, second });
        await unitOfWork.SaveChangesAsync();

        Assert.Equal(2, await context.Set<TestAggregate>().IgnoreQueryFilters().CountAsync());
    }
}
