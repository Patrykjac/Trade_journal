using TradeJournal.Core.Exceptions;

namespace TradeJournal.Core;

public class DayEntry
{
    public const int MaxNoteLength = 1000;

    public DateOnly Date { get; }
    public decimal Pnl { get; }
    public string Note { get; private set;} = string.Empty;

    public bool IsWinning => Pnl > 0;

    public bool IsLosing => Pnl < 0;

    public DayEntry(DateOnly date, decimal pnl)
    {
        if( date == default )
            throw new InvalidDayEntryException("Data wpisu nie może być pusta.");
        
        if (date > DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidDayEntryException("Data wpisu nie może być z przyszłości.");
        
        Date = date;
        Pnl = pnl;
    }

    public void SetNote(string? note)
    {
        var cleaned = note?.Trim() ?? string.Empty;

        if (cleaned.Length > MaxNoteLength)
            throw new InvalidDayEntryException(
                $"Notatka nie może mieć więcej niż {MaxNoteLength} znaków.");

        Note = cleaned;
    }

}