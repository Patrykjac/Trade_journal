namespace TradeJournal.Core.Rules;

// Preset reguł Trading Combine Topstepa: parametry zależą od rozmiaru konta.
// Źródło: Topstep Help Center (Trading Combine, Maximum Loss Limit), sprawdzone 2026-11-13.
// Zasady się zmieniają: przed użyciem sprawdź aktualne wartości na help.topstep.com.
public static class TopstepPreset
{
    // Udział najlepszego dnia w zysku. Część stron Topstepa podaje 50%, nowsze 55%: do potwierdzenia.
    public const decimal ConsistencyShare = 0.55m;

    public static decimal StartingBalance(TopstepSize size) => size switch
    {
        TopstepSize.Size50K => 50000m,
        TopstepSize.Size100K => 100000m,
        TopstepSize.Size150K => 150000m,
        _ => throw Unknown(size)
    };

    public static decimal MaximumLossLimit(TopstepSize size) => size switch
    {
        TopstepSize.Size50K => 2000m,
        TopstepSize.Size100K => 3000m,
        TopstepSize.Size150K => 4500m,
        _ => throw Unknown(size)
    };

    // Dzienny limit jest w Combine opcjonalny (wybierany przy zakupie).
    public static decimal DailyLossLimit(TopstepSize size) => size switch
    {
        TopstepSize.Size50K => 1000m,
        TopstepSize.Size100K => 2000m,
        TopstepSize.Size150K => 3000m,
        _ => throw Unknown(size)
    };

    public static RuleEngine CreateCombineRules(TopstepSize size, bool withDailyLossLimit = false)
    {
        var rules = new List<IRule>
        {
            new TopstepMaximumLossLimitRule(MaximumLossLimit(size)),
            new ConsistencyRule(ConsistencyShare)
        };

        if (withDailyLossLimit)
            rules.Add(new DailyLossLimitRule(DailyLossLimit(size)));

        return new RuleEngine(rules);
    }

    private static ArgumentOutOfRangeException Unknown(TopstepSize size) =>
        new(nameof(size), size, "Nieznany rozmiar konta Topstep.");
}