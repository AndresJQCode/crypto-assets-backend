namespace Domain.Exceptions;

public class SaveEntitiesException : Exception
{
    public SaveEntitiesException()
    { }

    public SaveEntitiesException(string message)
        : base(message)
    { }

    public SaveEntitiesException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
