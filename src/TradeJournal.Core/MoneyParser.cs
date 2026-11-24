using System.Globalization;

namespace TradeJournal.Core;

public static class MoneyParser
{
    // Przyjmuje kwoty z przecinkiem lub kropką, ze spacjami jako separatorem tysięcy.
    // Odrzuca zapisy niejednoznaczne, np. "1.234,50".
    public static bool TryParse(string? text, out decimal value)
    {
        value = 0m;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = text
            .Replace(" ", string.Empty)
            .Replace("\u00A0", string.Empty)
            .Replace(',', '.');

        return decimal.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }
}