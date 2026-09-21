namespace WebApplication1.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public decimal Balance { get; set; }
}
