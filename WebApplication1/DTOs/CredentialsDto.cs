using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos;

public record CredentialsDto(
    [Required, MaxLength(50)] string Name,
    [Required, MinLength(6)] string Password);