namespace TaskManagerApi.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid Email or Password") { }

}