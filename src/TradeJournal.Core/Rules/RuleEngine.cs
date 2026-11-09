using TradeJournal.Core.Exceptions;

namespace TradeJournal.Core.Rules;

public class RuleEngine
{
    private readonly List<IRule> _rules;

    public RuleEngine(IEnumerable<IRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        _rules = rules.ToList();

        if (_rules.Any(r => r is null))
            throw new InvalidRuleException("Lista reguł nie może zawierać pustych elementów.");
    }

    public IReadOnlyList<IRule> Rules => _rules;

    public IReadOnlyList<RuleResult> Evaluate(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return _rules.Select(rule => rule.Evaluate(account)).ToList();
    }

    public RuleStatus OverallStatus(Account account) =>
        Evaluate(account)
            .Select(result => result.Status)
            .DefaultIfEmpty(RuleStatus.Ok)
            .Max();
}