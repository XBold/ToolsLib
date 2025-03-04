namespace Tools.ValidationsAndExeptions;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public Dictionary<string, string> Errors { get; } = new();

    /// <summary>Add an error for the specific field (or anything else) that is not correct</summary>
    /// <param name="field">The name of the field that is not correct</param>
    /// <param name="message">Insert the reason why the field is not correct</param>
    public void AddError(string field, string message)
    {
        if (!Errors.ContainsKey(field))
        {
            Errors[field] = message;
        }
    }

    /// <summary>Get all the errors in a string</summary>
    /// <remarks>
    /// Example of how it can be used:
    /// <code>
    /// string errors = validationResult.GetFormattedErrors();
    /// Console.WriteLine(errors);
    /// </code>
    /// </remarks>
    /// <example>
    /// Example of how it can be used:
    /// <code>
    /// string errors = validationResult.GetFormattedErrors();
    /// Console.WriteLine(errors);
    /// </code>
    /// </example>
    public string GetFormattedErrors()
    {
        return string.Join(Environment.NewLine, Errors.Select(e => $"{e.Key}: {e.Value}"));
    }
}
