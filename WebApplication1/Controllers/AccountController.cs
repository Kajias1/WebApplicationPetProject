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

    /// <summary>
    /// Displays balance of the current user.
    /// </summary>
    /// <response code="200">Returns the current balance.</response>
    [HttpGet("balance")]
    public async Task<ActionResult<BalanceDto>> GetBalance()
        => new BalanceDto(await account.GetBalanceAsync(UserId));

    /// <summary>
    /// Deposits money into the current user's account.
    /// </summary>
    /// <param name="dto">The amount to deposit.</param>
    /// <response code="200">Returns the updated balance.</response>
    /// <response code="400">The amount is invalid.</response>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(BalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<BalanceDto>> Deposit(AmountDto dto)
        => ChangeBalance(dto.Amount, OperationType.Deposit);

    /// <summary>
    /// Withdraws money from the current user's account.
    /// </summary>
    /// <param name="dto">The amount to withdraw.</param>
    /// <response code="200">Returns the updated balance.</response>
    /// <response code="400">The amount is invalid.</response>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(BalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<BalanceDto>> Withdraw(AmountDto dto)
        => ChangeBalance(dto.Amount, OperationType.Withdraw);

    private async Task<ActionResult<BalanceDto>> ChangeBalance(decimal amount, OperationType type)
        => new BalanceDto(await account.ChangeBalanceAsync(UserId, amount, type));
}