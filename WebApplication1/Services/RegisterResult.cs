using WebApplication1.Models;

namespace WebApplication1.Services;

public enum RegisterStatus { Success, NameTaken }

public record RegisterResult(RegisterStatus Status, User? User = null);