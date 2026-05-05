namespace TaskManagerApi.Exceptions;

public class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException() : base("Expired or Revoked Refresh token, Try login")
    {

    }
}