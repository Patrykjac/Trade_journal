using TradeJournal.Core.Exceptions;

namespace TradeJournal.Core;

public class Account
{
    private readonly List<DayEntry> _entries = new();

    public string Name { get; }
    public decimal StartingBalance { get; }
    public IReadOnlyList<DayEntry> Entries => _entries;

    public Account(string name, decimal startingBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidAccountException("Nazwa konta nie może być pusta.");
        
        if (startingBalance <= 0)
            throw new InvalidAccountException("Saldo początkowe musi być większe od 0.");
        Name = name.Trim();
        StartingBalance = startingBalance;
    }

    public void AddEntry(DayEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (_entries.Any(e => e.Date == entry.Date))
            throw new DuplicateEntryException(entry.Date);

        _entries.Add(entry);
    }
    public int TradingDays => _entries.Count;
    public decimal TotalPnl => _entries.Sum(e => e.Pnl);
    public decimal CurrentBalance => StartingBalance + TotalPnl;

    public decimal WinRate =>
        TradingDays == 0
            ? 0m
            : (decimal)_entries.Count(e => e.IsWinning) / TradingDays * 100m;
    public decimal AverageWinningDay => AveragePnl(e => e.IsWinning);
    public decimal AverageLosingDay => AveragePnl(e => e.IsLosing);

    public decimal GrossProfit => _entries.Where(e => e.IsWinning).Sum(e => e.Pnl);
    public decimal GrossLoss => _entries.Where(e => e.IsLosing).Sum(e => -e.Pnl);
    public decimal? ProfitFactor =>
        GrossLoss == 0 ? null : GrossProfit/GrossLoss;
    public DayEntry? BestDay => _entries.MaxBy(e => e.Pnl);
    public DayEntry? WorstDay => _entries.MinBy(e => e.Pnl);

    public IReadOnlyList<EquityPoint> GetEquityCurve()
    {
        var balance = StartingBalance;
        var curve = new List<EquityPoint>();

        foreach(var entry in _entries.OrderBy(e => e.Date))
        {
            balance += entry.Pnl;
            curve.Add(new EquityPoint(entry.Date, balance));
        }

        return curve;
    }

    public decimal MaxDrawdown
    {
        get
        {
            var peak = StartingBalance;
            var maxDrawdown = 0m;

            foreach (var point in GetEquityCurve())
            {
                peak = Math.Max(peak, point.Balance);
                maxDrawdown = Math.Max(maxDrawdown, peak - point.Balance);
            }

            return maxDrawdown;
        }
    }

    private decimal AveragePnl(Func<DayEntry, bool> filter)
    {
        var days = _entries.Where(filter).ToList();
        return days.Count == 0 ? 0m : days.Average(e => e.Pnl);
    }
}