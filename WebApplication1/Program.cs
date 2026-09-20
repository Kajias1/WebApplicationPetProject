using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/hello", () => "Hello World!")
    .WithName("GetHelloWorld");

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
            e.HasIndex(u => u.Name).IsUnique();          // имена не повторяются
            e.Property(u => u.Balance).HasPrecision(18, 2);
        });

        builder.Entity<Operation>()
            .Property(o => o.Amount).HasPrecision(18, 2);
    }
}