namespace Invoice.Domain.Exceptions;

public class UniqueConstraintViolationException(string? constraintName, Exception? innerException = null)
    : Exception($"Unique constraint violated: {constraintName}", innerException)
{
    public string? ConstraintName { get; } = constraintName;
}
