namespace WebApplication1.Services;

public enum ChangeBalanceStatus { Success, InvalidAmount, InsufficientFunds }

public record ChangeBalanceResult(ChangeBalanceStatus Status, decimal Balance = 0);