namespace EnglishLearning.Domain.Exceptions;

public class UnauthorizedSessionAccessException : DomainException
{
    public UnauthorizedSessionAccessException(Guid sessionId)
        : base($"Access to conversation session '{sessionId}' is not permitted.")
    {
    }
}
