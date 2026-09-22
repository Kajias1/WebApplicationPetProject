using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class AuthService(AppDbContext db, IPasswordHasher<User> hasher) : IAuthService
{
    public async Task<User> RegisterAsync(CredentialsDto dto)
    {
        var name = dto.Name.Trim();
        if (await db.Users.AnyAsync(u => u.Name == name))
            throw new NameTakenException();

        var user = new User { Name = name };
        user.PasswordHash = hasher.HashPassword(user, dto.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    public async Task<User> ValidateCredentialsAsync(CredentialsDto dto)
    {
        var name = dto.Name.Trim();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Name == name);

        if (user is null ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password)
            == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        return user;
    }
}