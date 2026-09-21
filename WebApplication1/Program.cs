using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "bank.auth";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public decimal Balance { get; set; }
}

public enum OperationType { Deposit, Withdraw }

public class Operation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public OperationType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record CredentialsDto(
    [Required, MaxLength(50)] string Name,
    [Required, MinLength(6)] string Password);

public record AmountDto(decimal Amount);
public record BalanceDto(decimal Balance);

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Operation> Operations => Set<Operation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(e =>
        {
            e.Property(u => u.Name).HasMaxLength(50);
            e.HasIndex(u => u.Name).IsUnique();
            e.Property(u => u.Balance).HasPrecision(18, 2);
        });

        builder.Entity<Operation>()
            .Property(o => o.Amount).HasPrecision(18, 2);
    }
}

public enum RegisterStatus { Success, NameTaken }

public record RegisterResult(RegisterStatus Status, User? User = null);

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(CredentialsDto dto);
    Task<User?> ValidateCredentialsAsync(CredentialsDto dto);
}

public class AuthService(AppDbContext db, IPasswordHasher<User> hasher) : IAuthService
{
    public async Task<RegisterResult> RegisterAsync(CredentialsDto dto)
    {
        var name = dto.Name.Trim();
        if (await db.Users.AnyAsync(u => u.Name == name))
            return new RegisterResult(RegisterStatus.NameTaken);

        var user = new User { Name = name };
        user.PasswordHash = hasher.HashPassword(user, dto.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return new RegisterResult(RegisterStatus.Success, user);
    }

    public async Task<User?> ValidateCredentialsAsync(CredentialsDto dto)
    {
        var name = dto.Name.Trim();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Name == name);

        if (user is null ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password)
            == PasswordVerificationResult.Failed)
            return null;

        return user;
    }
}