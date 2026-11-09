using TradeJournal.Core.Exceptions;

namespace TradeJournal.Core.Rules;

public class DailyLossLimitRule : IRule
{
    // ostrzeżenie, gdy strata dnia sięgnęła 80% limitu
    private const decimal WarningFraction = 0.8m;

    public decimal MaxDailyLoss { get; }

    public string Name => "Dzienny limit straty";

    public DailyLossLimitRule(decimal maxDailyLoss)
    {
        if (maxDailyLoss <= 0)
            throw new InvalidRuleException("Dzienny limit straty musi być większy od zera.");

        MaxDailyLoss = maxDailyLoss;
    }

    public RuleResult Evaluate(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        var latest = account.Entries.MaxBy(e => e.Date);

        if (latest is null)
            return new RuleResult(Name, RuleStatus.Ok, "Brak wpisów.");

        var loss = latest.Pnl < 0 ? -latest.Pnl : 0m;

        if (loss >= MaxDailyLoss)
            return new RuleResult(Name, RuleStatus.Violated,
                $"Strata {loss:N2} dnia {latest.Date:yyyy-MM-dd} osiągnęła limit {MaxDailyLoss:N2}.");

        if (loss >= MaxDailyLoss * WarningFraction)
            return new RuleResult(Name, RuleStatus.Warning,
                $"Strata {loss:N2} dnia {latest.Date:yyyy-MM-dd} zbliża się do limitu {MaxDailyLoss:N2}.");

        return new RuleResult(Name, RuleStatus.Ok,
            $"Strata dnia {latest.Date:yyyy-MM-dd}: {loss:N2} z limitu {MaxDailyLoss:N2}.");
    }
}