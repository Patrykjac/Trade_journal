namespace TradeJournal.Core.Rules;

public class TrailingEndOfDayDrawdownRule : DrawdownRule
{
    public TrailingEndOfDayDrawdownRule(decimal maxDrawdown) : base(maxDrawdown)
    {
    }

    public override string Name => "Drawdown trailing (koniec dnia)";

    protected override decimal GetFloor(Account account)
    {
        var peak = account.StartingBalance;

        foreach (var point in account.GetEquityCurve())
        {
            peak = Math.Max(peak, point.Balance);
        }

        return peak - MaxDrawdown;
    }
}