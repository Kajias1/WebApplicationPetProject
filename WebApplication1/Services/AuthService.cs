using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class AuthService(AppDbContext db, IPasswordHasher<User> hasher) : IAuthService
{
    private const string DummyHash =
        "AQAAAAIAAYagAAAAELZzz6N0GmyTbenCN+JnF/uJasC0i0+vrpb8HWoM33EzPwOIR8+f68F257+q8qbguQ==";
    
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

        // Dummy user for constant verification time for protecting from timing attacks
        var target = user ?? new User { PasswordHash = DummyHash };
        var result = hasher.VerifyHashedPassword(target, target.PasswordHash!, dto.Password);

        if (user is null || result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        return user;
    }
}