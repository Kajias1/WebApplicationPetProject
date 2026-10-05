using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dtos;

public record CredentialsDto(
    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    [MaxLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
    string Name,

    [Required(ErrorMessage = "Пароль обязателен для заполнения")]
    [MinLength(6, ErrorMessage = "Пароль должен содержать не менее 6 символов")]
    string Password);