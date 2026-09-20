using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, IPasswordHasher<User> hasher) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(CredentialsDto dto)
    {
        var name = dto.Name.Trim();
        if (await db.Users.AnyAsync(u => u.Name == name))
            return Conflict("Пользователь с таким именем уже существует");

        var user = new User { Name = name };
        user.PasswordHash = hasher.HashPassword(user, dto.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new { user.Id, user.Name });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(CredentialsDto dto)
    {
        var name = dto.Name.Trim();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Name == name);

        if (user is null ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password)
            == PasswordVerificationResult.Failed)
            return Unauthorized("Неверное имя или пароль");

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

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}