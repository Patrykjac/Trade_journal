namespace TradeJournal.Core.Rules;

public class StaticDrawdownRule : DrawdownRule
{
    public StaticDrawdownRule(decimal maxDrawdown) : base(maxDrawdown)
    {
    }

    public override string Name => "Drawdown statyczny";

    protected override decimal GetFloor(Account account) =>
        account.StartingBalance - MaxDrawdown;
}