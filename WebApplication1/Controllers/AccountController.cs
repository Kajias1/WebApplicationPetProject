using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AccountController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("balance")]
    public async Task<ActionResult<BalanceDto>> GetBalance()
    {
        var userId = UserId;
        var balance = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Balance)
            .SingleAsync();

        return new BalanceDto(balance);
    }

    [HttpPost("deposit")]
    public Task<IActionResult> Deposit(AmountDto dto)
        => ChangeBalance(dto.Amount, OperationType.Deposit);

    [HttpPost("withdraw")]
    public Task<IActionResult> Withdraw(AmountDto dto)
        => ChangeBalance(dto.Amount, OperationType.Withdraw);

    private async Task<IActionResult> ChangeBalance(decimal amount, OperationType type)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            return BadRequest("Сумма должна быть больше нуля, не более 2 знаков после запятой");

        var userId = UserId;
        var delta = type == OperationType.Deposit ? amount : -amount;

        await using var tx = await db.Database.BeginTransactionAsync();

        var updated = await db.Users
            .Where(u => u.Id == userId && u.Balance + delta >= 0)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Balance, u => u.Balance + delta));

        if (updated == 0)
            return BadRequest("Недостаточно средств");

        db.Operations.Add(new Operation { UserId = userId, Type = type, Amount = amount });
        await db.SaveChangesAsync();

        var balance = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Balance)
            .SingleAsync();

        await tx.CommitAsync();
        return Ok(new BalanceDto(balance));
    }
}