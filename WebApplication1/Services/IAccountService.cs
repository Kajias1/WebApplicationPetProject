using WebApplication1.Models;

namespace WebApplication1.Services;

public interface IAccountService
{
    Task<decimal> GetBalanceAsync(int userId);
    Task<decimal> ChangeBalanceAsync(int userId, decimal amount, OperationType type);
}