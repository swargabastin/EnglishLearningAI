namespace EnglishLearning.Domain.Exceptions;

public class InvalidSessionStateException : DomainException
{
    public InvalidSessionStateException(string message) : base(message)
    {
    }
}
