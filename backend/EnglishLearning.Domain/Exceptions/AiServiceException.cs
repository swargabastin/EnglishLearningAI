namespace EnglishLearning.Domain.Exceptions;

public class AiServiceException : DomainException
{
    public AiServiceException(string message, Exception? innerException = null)
        : base(message, innerException!)
    {
    }
}
