namespace WebApplication1.Services;

public abstract class AppException(string message) : Exception(message);

public class NameTakenException()
    : AppException("Пользователь с таким именем уже существует");

public class InvalidCredentialsException()
    : AppException("Неверное имя или пароль");

public class InsufficientFundsException()
    : AppException("Недостаточно средств");

public class InvalidAmountException()
    : AppException("Сумма должна быть больше нуля, не более 2 знаков после запятой");