namespace TradeJournal.Core.Rules;

// Maximum Loss Limit w stylu Topstep: trailing liczony od szczytu salda z końca dnia,
// ale poziom nigdy nie wzrasta powyżej salda początkowego (blokada).
public class TopstepMaximumLossLimitRule : TrailingEndOfDayDrawdownRule
{
    public TopstepMaximumLossLimitRule(decimal maxLossLimit) : base(maxLossLimit)
    {
    }

    public override string Name => "Topstep Maximum Loss Limit";

    protected override decimal GetFloor(Account account) =>
        Math.Min(base.GetFloor(account), account.StartingBalance);
}