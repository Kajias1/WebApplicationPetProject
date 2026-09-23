using WebApplication1.Models;

namespace WebApplication1.Services;

public interface ITokenService
{
    string CreateToken(User user);
}