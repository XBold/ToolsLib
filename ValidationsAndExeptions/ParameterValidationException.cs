namespace Tools.ValidationsAndExeptions;

public class ParameterValidationException : Exception
{
    public ValidationResult ValidationResult { get; }

    public ParameterValidationException(string message, ValidationResult validationResult)
        : base(message)
    {
        ValidationResult = validationResult;
    }
}