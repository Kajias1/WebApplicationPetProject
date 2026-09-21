using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dtos;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AccountController(IAccountService account) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("balance")]
    public async Task<ActionResult<BalanceDto>> GetBalance()
        => new BalanceDto(await account.GetBalanceAsync(UserId));

    [HttpPost("deposit")]
    public Task<IActionResult> Deposit(AmountDto dto)
        => ChangeBalance(dto.Amount, OperationType.Deposit);

    [HttpPost("withdraw")]
    public Task<IActionResult> Withdraw(AmountDto dto)
        => ChangeBalance(dto.Amount, OperationType.Withdraw);

    private async Task<IActionResult> ChangeBalance(decimal amount, OperationType type)
    {
        var result = await account.ChangeBalanceAsync(UserId, amount, type);

        return result.Status switch
        {
            ChangeBalanceStatus.InvalidAmount =>
                BadRequest("Сумма должна быть больше нуля, не более 2 знаков после запятой"),
            ChangeBalanceStatus.InsufficientFunds =>
                BadRequest("Недостаточно средств"),
            _ => Ok(new BalanceDto(result.Balance))
        };
    }
}