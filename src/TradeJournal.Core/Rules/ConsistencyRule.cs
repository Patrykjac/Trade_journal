using TradeJournal.Core.Exceptions;

namespace TradeJournal.Core.Rules;

// Najlepszy dzień nie może przekraczać zadanego udziału w łącznym zysku netto.
// Przekroczenie to ostrzeżenie (w Topstepie podnosi cel zysku), a nie koniec konta.
public class ConsistencyRule : IRule
{
    public decimal MaxBestDayShare { get; }

    public string Name => "Reguła konsystencji";

    public ConsistencyRule(decimal maxBestDayShare)
    {
        if (maxBestDayShare <= 0 || maxBestDayShare > 1)
            throw new InvalidRuleException("Udział najlepszego dnia musi mieścić się w przedziale (0, 1].");

        MaxBestDayShare = maxBestDayShare;
    }

    public RuleResult Evaluate(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        var best = account.BestDay;
        var total = account.TotalPnl;

        if (best is null || total <= 0)
            return new RuleResult(Name, RuleStatus.Ok, "Brak zysku netto do oceny konsystencji.");

        var share = best.Pnl / total;

        if (share <= MaxBestDayShare)
            return new RuleResult(Name, RuleStatus.Ok,
                $"Najlepszy dzień to {share:P0} zysku (limit {MaxBestDayShare:P0}).");

        var missing = best.Pnl / MaxBestDayShare - total;

        return new RuleResult(Name, RuleStatus.Warning,
            $"Najlepszy dzień to {share:P0} zysku (limit {MaxBestDayShare:P0}). Brakuje {missing:N2} zysku, aby wrócić do limitu.");
    }
}