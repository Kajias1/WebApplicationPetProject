using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Tests;

public class AccountServiceTests
{
    [Fact]
    public async Task ChangeBalanceAsync_Deposit_IncreasesBalance()
    {
        await using var db = TestHelpers.CreateDbContext();
        db.Users.Add(new User { Id = 1, Name = "alice", Balance = 100m });
        await db.SaveChangesAsync();

        var service = new AccountService(db);

        var balance = await service.ChangeBalanceAsync(1, 50m, OperationType.Deposit);

        Assert.Equal(150m, balance);
    }

    [Fact]
    public async Task ChangeBalanceAsync_WithdrawMoreThanBalance_ThrowsInsufficientFunds()
    {
        await using var db = TestHelpers.CreateDbContext();
        db.Users.Add(new User { Id = 1, Name = "alice", Balance = 30m });
        await db.SaveChangesAsync();

        var service = new AccountService(db);

        await Assert.ThrowsAsync<InsufficientFundsException>(
            () => service.ChangeBalanceAsync(1, 50m, OperationType.Withdraw));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(10.123)]
    public async Task ChangeBalanceAsync_InvalidAmount_ThrowsInvalidAmountException(decimal amount)
    {
        await using var db = TestHelpers.CreateDbContext();
        db.Users.Add(new User { Id = 1, Name = "alice", Balance = 100m });
        await db.SaveChangesAsync();

        var service = new AccountService(db);

        await Assert.ThrowsAsync<InvalidAmountException>(
            () => service.ChangeBalanceAsync(1, amount, OperationType.Deposit));
    }
}