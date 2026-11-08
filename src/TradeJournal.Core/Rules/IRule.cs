namespace TradeJournal.Core.Rules;

public interface IRule
{
    string Name { get; }

    RuleResult Evaluate(Account account);
}