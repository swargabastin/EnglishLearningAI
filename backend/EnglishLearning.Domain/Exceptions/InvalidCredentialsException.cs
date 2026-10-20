namespace EnglishLearning.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException(string message = "Invalid email or password.")
        : base(message)
    {
    }
}
