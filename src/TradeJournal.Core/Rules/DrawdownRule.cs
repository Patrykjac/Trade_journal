using TradeJournal.Core.Exceptions;

namespace TradeJournal.Core.Rules;

public abstract class DrawdownRule : IRule
{
    // ostrzeżenie, gdy do limitu zostało nie więcej niż 20% dozwolonego drawdownu
    private const decimal WarningFraction = 0.2m;

    public decimal MaxDrawdown { get; }

    public abstract string Name { get; }

    protected DrawdownRule(decimal maxDrawdown)
    {
        if (maxDrawdown <= 0)
            throw new InvalidRuleException("Limit drawdownu musi być większy od zera.");

        MaxDrawdown = maxDrawdown;
    }

    // Poziom salda, poniżej którego konto jest stracone. Każdy wariant liczy go inaczej.
    protected abstract decimal GetFloor(Account account);

    public RuleResult Evaluate(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        var floor = GetFloor(account);
        var remaining = account.CurrentBalance - floor;

        if (remaining <= 0)
            return new RuleResult(Name, RuleStatus.Violated,
                $"Saldo {account.CurrentBalance:N2} osiągnęło poziom limitu {floor:N2}.");

        if (remaining <= MaxDrawdown * WarningFraction)
            return new RuleResult(Name, RuleStatus.Warning,
                $"Do limitu {floor:N2} zostało tylko {remaining:N2}.");

        return new RuleResult(Name, RuleStatus.Ok,
            $"Do limitu {floor:N2} zostało {remaining:N2}.");
    }
}