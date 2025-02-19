using Core.Domain;
using Core.Exceptions;

namespace Core.Tests.Domain;

public class AccountTests
{
    private readonly TimeSpan sessionLifeSpan = TimeSpan.FromMinutes(30);
    private readonly Account testAccount = new("username", "email", "password");

    [Fact]
    public void HasBeenActivated_ShouldReturnTrue_WhenAccountIsNew()
    {
        Assert.False(testAccount.HasBeenActivated());
    }

    [Fact]
    public void HasBeenActivated_ShouldReturnTrue_WhenAccountIsActivated()
    {
        testAccount.Activate();

        Assert.True(testAccount.HasBeenActivated());
    }

    [Fact]
    public void ChangePassword_ShouldUpdatePassword()
    {
        var newPassword = "new-password";

        testAccount.ChangePassword(newPassword);

        Assert.Equal(newPassword, testAccount.Password);
    }

    [Fact]
    public void AssignRole_ShouldFail_WhenAccountAlreadyHasRole()
    {
        testAccount.AssignRole(Role.Admin, Guid.Empty);

        var result = testAccount.AssignRole(Role.ProblemsCreator, Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.IsType<RoleAlreadyAssigned>(result.Exception);
    }

    [Fact]
    public void AssignRole_ShouldAssignRole_WhenAccountHasNoRoleAssignedAndIsNotIssuer()
    {
        var result = testAccount.AssignRole(Role.Admin, Guid.Empty);

        Assert.True(result.IsSuccess);
        Assert.Equal(Role.Admin, testAccount.Role);
    }

    [Fact]
    public void AssignRole_ShouldRemoveAllSessions_WhenAccountHasNoRoleAssignedAndIsNotIssuer()
    {
        var firstToken = CreateSessionAndGetToken();
        var secondToken = CreateSessionAndGetToken();

        testAccount.AssignRole(Role.Admin, Guid.Empty);

        AssertSessionRemoved(firstToken);
        AssertSessionRemoved(secondToken);
    }

    [Fact]
    public void AssignRole_Fail_WhenAccountIsIssuer()
    {
        var result = testAccount.AssignRole(Role.Admin, testAccount.Id);

        Assert.True(result.IsFailure);
        Assert.IsType<CannotManageOwn<Role>>(result.Exception);
    }

    [Fact]
    public void RemoveRole_ShouldSucceedAndRemoveRole_WhenAccountIsNotIssuer()
    {
        testAccount.AssignRole(Role.Admin, Guid.Empty);

        var result = testAccount.RemoveRole(Guid.Empty);

        Assert.True(result.IsSuccess);
        Assert.Equal(Role.None, testAccount.Role);
    }

    [Fact]
    public void RemoveRole_ShouldFail_WhenAccountIsIssuer()
    {
        var result = testAccount.RemoveRole(testAccount.Id);

        Assert.True(result.IsFailure);
        Assert.IsType<CannotManageOwn<Role>>(result.Exception);
    }

    [Fact]
    public void CreateSession_ShouldFail_WhenAccountHasNotBeenActivatedYet()
    {
        var result = testAccount.CreateSession(DateTime.MinValue);

        Assert.True(result.IsFailure);
        Assert.IsType<AccountNotActivated>(result.Exception);
    }

    [Fact]
    public void CreateSession_ShouldSucceed_WhenAccountHasBeenActivated()
    {
        testAccount.Activate();

        var result = testAccount.CreateSession(DateTime.MinValue);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public void GetSessionCurrentToken_ShouldReturnCorrectToken_WhenSessionHasBeenCreated()
    {
        testAccount.Activate();

        var expected = testAccount.CreateSession(DateTime.MinValue).Value;

        var actual = testAccount.GetSessionCurrentToken(expected.SessionId);

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
    }

    [Fact]
    public void DestroySession_ShouldRemoveCorrect()
    {
        testAccount.Activate();

        var firstToken = CreateSessionAndGetToken();

        var secondToken = CreateSessionAndGetToken();

        testAccount.DestroySession(firstToken.SessionId);

        AssertSessionRemoved(firstToken);
        AssertSessionExists(secondToken);
    }

    [Fact]
    public void DestroyAllSessions_ShouldRemoveAllSessions()
    {
        testAccount.Activate();

        var firstToken = CreateSessionAndGetToken();

        var secondToken = CreateSessionAndGetToken();

        testAccount.DestroyAllSessions();

        AssertSessionRemoved(firstToken);
        AssertSessionRemoved(secondToken);
    }

    [Fact]
    public void RefreshToken_ShouldFail_WhenSessionLinkedToGivenTokenDoesNotExist()
    {
        var testToken = new RefreshToken(Guid.Empty, Guid.Empty);

        var result = testAccount.RefreshSession(testToken, DateTime.MinValue);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<AuthSession>>(result.Exception);
    }

    [Fact]
    public void RefreshToken_ShouldFailAndDestroyAllSessions_WhenGivenTokenIsNotTheCurrentOne()
    {
        var firstSession = CreateSessionAndGetToken();
        var secondSession = CreateSessionAndGetToken();

        var testToken = new RefreshToken(secondSession.SessionId);

        var result = testAccount.RefreshSession(testToken, DateTime.MinValue);

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidToken>(result.Exception);
        AssertSessionRemoved(firstSession);
        AssertSessionRemoved(secondSession);
    }

    [Fact]
    public void RefreshToken_ShouldFailAndDestroySession_WhenGivenTokenIsCurrentOneAndSessionIsInactive()
    {
        var testSession = CreateSessionAndGetToken();
        var otherSession = CreateSessionAndGetToken();

        var result = testAccount.RefreshSession(testSession, DateTime.MinValue + sessionLifeSpan);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<AuthSession>>(result.Exception);
        AssertSessionExists(otherSession);
        AssertSessionRemoved(testSession);
    }

    [Fact]
    public void RefreshToken_ShouldSucceedAndReturnValidRefreshToken_WhenGivenTokenIsCurrentOneAndSessionIsActive()
    {
        var testSession = CreateSessionAndGetToken();

        var result = testAccount.RefreshSession(testSession, DateTime.MinValue);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(testSession.SessionId, result.Value.SessionId);
    }


    [Fact]
    public void ResetPassword_ShouldChangePassword()
    {
        var newPasswordHash = "new-password-hash";
        testAccount.ResetPassword(newPasswordHash);

        Assert.Equal(newPasswordHash, testAccount.Password);
    }

    [Fact]
    public void ResetPassword_ShouldRemoveAllSessions()
    {
        var firstToken = CreateSessionAndGetToken();

        var secondToken = CreateSessionAndGetToken();

        testAccount.ResetPassword("new-password-hash");

        AssertSessionRemoved(firstToken);
        AssertSessionRemoved(secondToken);
    }

    private RefreshToken CreateSessionAndGetToken()
    {
        testAccount.Activate();
        return testAccount.CreateSession(DateTime.MinValue).Value;
    }

    private void AssertSessionRemoved(RefreshToken token)
    {
        Assert.Null(testAccount.GetSessionCurrentToken(token.SessionId));
    }

    private void AssertSessionExists(RefreshToken token)
    {
        Assert.NotNull(testAccount.GetSessionCurrentToken(token.SessionId));
    }
}
