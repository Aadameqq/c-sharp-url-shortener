using Core.Domain;
using Core.Exceptions;

namespace Core.Tests.Domain;

public class AuthSessionTests
{
    private readonly TimeSpan sessionLifeSpan = TimeSpan.FromMinutes(30);
    private readonly DateTime testNow = DateTime.MinValue;
    private readonly AuthSession testSession;

    public AuthSessionTests()
    {
        testSession = new AuthSession(Guid.Empty, testNow);
    }

    [Fact]
    public void IsActive_ShouldBeTrue_WhenCurrentDateIsLowerThanExpirationDate()
    {
        Assert.True(testSession.IsActive(testNow));
        Assert.True(
            testSession.IsActive(testNow + sessionLifeSpan.Subtract(TimeSpan.FromMinutes(1)))
        );
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenCurrentDateIsGreaterThanOrEqualExpirationDate()
    {
        Assert.False(testSession.IsActive(testNow + sessionLifeSpan));
        Assert.False(testSession.IsActive(testNow + sessionLifeSpan.Add(TimeSpan.FromMinutes(70))));
    }

    [Fact]
    public void Refresh_ShouldFail_WhenSessionIsNotActive()
    {
        var result = testSession.Refresh(testNow + sessionLifeSpan);

        Assert.True(result.IsFailure);
        Assert.IsType<SessionInactive>(result.Exception);
    }

    [Fact]
    public void Refresh_ShouldSucceedAndReturnNewRefreshToken_WhenSessionIsActive()
    {
        var result = testSession.Refresh(
            testNow + sessionLifeSpan.Subtract(TimeSpan.FromMinutes(1))
        );

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(result.Value.SessionId, testSession.Id);
    }

    [Fact]
    public void Refresh_ShouldChangeCurrentTokenAndExpirationDate_WhenSessionIsActive()
    {
        var oldToken = testSession.CurrentToken;

        var refreshTime = testNow + sessionLifeSpan.Subtract(TimeSpan.FromMinutes(1));

        var result = testSession.Refresh(refreshTime);

        Assert.NotEqual(oldToken.Id, testSession.CurrentToken.Id);
        Assert.Equal(result.Value.Id, testSession.CurrentToken.Id);
        Assert.True(
            testSession.IsActive(refreshTime + sessionLifeSpan.Subtract(TimeSpan.FromMinutes(1)))
        );
        Assert.False(testSession.IsActive(refreshTime + sessionLifeSpan));
    }
}
