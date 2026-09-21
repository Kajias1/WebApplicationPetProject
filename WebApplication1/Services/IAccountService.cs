using WebApplication1.Models;

namespace WebApplication1.Services;

public interface IAccountService
{
    Task<decimal> GetBalanceAsync(int userId);
    Task<ChangeBalanceResult> ChangeBalanceAsync(int userId, decimal amount, OperationType type);
}