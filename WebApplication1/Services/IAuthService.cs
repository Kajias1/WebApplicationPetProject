using WebApplication1.Dtos;
using WebApplication1.Models;

namespace WebApplication1.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(CredentialsDto dto);
    Task<User> ValidateCredentialsAsync(CredentialsDto dto);
}