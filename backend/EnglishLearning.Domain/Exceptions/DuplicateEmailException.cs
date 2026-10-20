namespace EnglishLearning.Domain.Exceptions;

public class DuplicateEmailException : DomainException
{
    public DuplicateEmailException(string email)
        : base($"An account with email '{email}' already exists.")
    {
    }
}
