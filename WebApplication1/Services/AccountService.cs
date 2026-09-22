using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class AccountService(AppDbContext db) : IAccountService
{
    public Task<decimal> GetBalanceAsync(int userId)
        => db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Balance)
            .SingleAsync();

    public async Task<decimal> ChangeBalanceAsync(
        int userId, decimal amount, OperationType type)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new InvalidAmountException();

        var delta = type == OperationType.Deposit ? amount : -amount;

        await using var tx = await db.Database.BeginTransactionAsync();

        var updated = await db.Users
            .Where(u => u.Id == userId && u.Balance + delta >= 0)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Balance, u => u.Balance + delta));

        if (updated == 0)
            throw new InsufficientFundsException();

        db.Operations.Add(new Operation { UserId = userId, Type = type, Amount = amount });
        await db.SaveChangesAsync();

        var balance = await GetBalanceAsync(userId);
        await tx.CommitAsync();
        return balance;
    }
}