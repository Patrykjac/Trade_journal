using System.Globalization;
using TradeJournal.Core;

namespace TradeJournal.Tests;

public class MoneyParserTests
{
    [Theory]
    [InlineData("50000", "50000")]
    [InlineData("50 000", "50000")]
    [InlineData("50000,5", "50000.5")]
    [InlineData("50000.5", "50000.5")]
    [InlineData("-12,34", "-12.34")]
    [InlineData("  100  ", "100")]
    [InlineData("0", "0")]
    public void TryParse_AcceptsPolishAndInvariantFormats(string input, string expected)
    {
        var ok = MoneyParser.TryParse(input, out var value);

        Assert.True(ok);
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1.234,50")]
    [InlineData("12,3,4")]
    [InlineData("1e5")]
    public void TryParse_RejectsInvalidText(string? input)
    {
        Assert.False(MoneyParser.TryParse(input, out _));
    }
}