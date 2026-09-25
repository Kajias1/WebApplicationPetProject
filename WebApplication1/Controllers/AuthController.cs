using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApplication1.Dtos;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Endpoints for registering and logging in using JWT authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth, ITokenService tokens) : ControllerBase
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="dto">The desired name and password for the new account.</param>
    /// <returns>The newly created user's id and name.</returns>
    /// <response code="200">The user was created successfully.</response>
    /// <response code="400">The request failed validation (e.g. password too short).</response>
    /// <response code="409">A user with this name already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(CredentialsDto dto)
    {
        var user = await auth.RegisterAsync(dto);
        return Ok(new { user.Id, user.Name });
    }

    /// <summary>
    /// Logs a user in and issues a JWT.
    /// </summary>
    /// <param name="dto">The user's name and password.</param>
    /// <returns>A bearer token to use in the <c>Authorization</c> header of later requests.</returns>
    /// <response code="200">Login succeeded; the response contains the JWT.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The name or password is incorrect.</response>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(CredentialsDto dto)
    {
        var user = await auth.ValidateCredentialsAsync(dto);
        var token = tokens.CreateToken(user);

        return Ok(new { token, user.Id, user.Name });
    }
}