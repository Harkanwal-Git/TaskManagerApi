namespace TaskManagerApi.Exceptions;

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email) : base($"{email} already exists")
    {

    }
}