using TradeJournal.Core;

namespace TradeJournal.Tests;

internal static class TestHelpers
{
    // Konto z saldem początkowym 50 000 i kolejnymi dniami o podanym PnL.
    public static Account AccountWith(params decimal[] dailyPnls)
    {
        var account = new Account("Konto testowe", 50000m);

        for (var i = 0; i < dailyPnls.Length; i++)
        {
            account.AddEntry(new DayEntry(new DateOnly(2025, 1, 10 + i), dailyPnls[i]));
        }

        return account;
    }
}