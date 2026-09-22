using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication1.Dtos;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_NewName_CreatesUser()
    {
        // Arrange
        await using var db = TestHelpers.CreateDbContext();
        var hasherMock = new Mock<IPasswordHasher<User>>();
        hasherMock
            .Setup(h => h.HashPassword(It.IsAny<User>(), "secret123"))
            .Returns("hashed-value");

        var service = new AuthService(db, hasherMock.Object);
        var dto = new CredentialsDto("alice", "secret123");

        // Act
        var user = await service.RegisterAsync(dto);

        // Assert
        Assert.Equal("alice", user.Name);
        Assert.Equal("hashed-value", user.PasswordHash);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_NameAlreadyTaken_ThrowsNameTakenException()
    {
        // Arrange
        await using var db = TestHelpers.CreateDbContext();
        db.Users.Add(new User { Name = "alice", PasswordHash = "x" });
        await db.SaveChangesAsync();

        var hasherMock = new Mock<IPasswordHasher<User>>();
        var service = new AuthService(db, hasherMock.Object);
        var dto = new CredentialsDto("alice", "secret123");

        // Act & Assert
        await Assert.ThrowsAsync<NameTakenException>(() => service.RegisterAsync(dto));
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WrongPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        await using var db = TestHelpers.CreateDbContext();
        db.Users.Add(new User { Name = "alice", PasswordHash = "hashed" });
        await db.SaveChangesAsync();

        var hasherMock = new Mock<IPasswordHasher<User>>();
        hasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), "hashed", "wrongpass"))
            .Returns(PasswordVerificationResult.Failed);

        var service = new AuthService(db, hasherMock.Object);
        var dto = new CredentialsDto("alice", "wrongpass");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.ValidateCredentialsAsync(dto));
    }
}