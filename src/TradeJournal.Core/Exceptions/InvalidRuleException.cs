namespace TradeJournal.Core.Exceptions;

public class InvalidRuleException : Exception
{
    public InvalidRuleException(string message) : base(message)
    {
    }
}