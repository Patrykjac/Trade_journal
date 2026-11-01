namespace TradeJournal.Core.Exceptions;

public class InvalidDayEntryException : Exception
{
    public InvalidDayEntryException(string message) : base(message)
    {
    }
}