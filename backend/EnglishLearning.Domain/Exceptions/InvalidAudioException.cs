namespace EnglishLearning.Domain.Exceptions;

public class InvalidAudioException : DomainException
{
    public InvalidAudioException(string message) : base(message)
    {
    }
}
