using LastHour.BuildingBlocks.Infrastructure.Time;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Time;

public class SystemClockTests
{
    [Fact]
    public void UtcNow_ReturnsCurrentUtcInstant()
    {
        var clock = new SystemClock();

        DateTime result = clock.UtcNow;

        Assert.True(Math.Abs((result - DateTime.UtcNow).TotalSeconds) < 5);
    }

    [Fact]
    public void UtcNow_IsUtcKind()
    {
        var clock = new SystemClock();

        Assert.Equal(DateTimeKind.Utc, clock.UtcNow.Kind);
    }
}

public class SystemDateTimeProviderTests
{
    [Fact]
    public void Now_ReturnsLocalTime()
    {
        var provider = new SystemDateTimeProvider();

        Assert.Equal(DateTimeKind.Local, provider.Now.Kind);
    }

    [Fact]
    public void UtcNow_ReturnsUtcTime()
    {
        var provider = new SystemDateTimeProvider();

        Assert.Equal(DateTimeKind.Utc, provider.UtcNow.Kind);
    }

    [Fact]
    public void UtcNowOffset_ReturnsUtcOffset()
    {
        var provider = new SystemDateTimeProvider();

        Assert.Equal(TimeSpan.Zero, provider.UtcNowOffset.Offset);
    }
}

public class DefaultCurrentUserTests
{
    [Fact]
    public void IsUnauthenticatedByDefault()
    {
        var user = new DefaultCurrentUser();

        Assert.False(user.IsAuthenticated);
        Assert.Null(user.UserId);
        Assert.Null(user.Name);
        Assert.Empty(user.Roles);
        Assert.False(user.IsInRole("admin"));
    }
}

public class DefaultCurrentTenantTests
{
    [Fact]
    public void IsUnavailableByDefault()
    {
        var tenant = new DefaultCurrentTenant();

        Assert.False(tenant.IsAvailable);
        Assert.Null(tenant.TenantId);
        Assert.Null(tenant.Name);
    }
}
