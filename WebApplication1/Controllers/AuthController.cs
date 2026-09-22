using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dtos;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

/// <summary>
/// Endpoints for registering, logging in, and logging out using cookie authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth) : ControllerBase
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
    /// Logs a user in and issues an authentication cookie.
    /// </summary>
    /// <param name="dto">The user's name and password.</param>
    /// <returns>The logged-in user's id and name.</returns>
    /// <response code="200">Login succeeded; an auth cookie is set on the response.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The name or password is incorrect.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(CredentialsDto dto)
    {
        var user = await auth.ValidateCredentialsAsync(dto);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(new { user.Id, user.Name });
    }

    /// <summary>
    /// Logs the current user out by clearing the authentication cookie.
    /// </summary>
    /// <response code="204">Logout succeeded; no content is returned.</response>
    /// <response code="401">No user is currently logged in.</response>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}