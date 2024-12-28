namespace Tools;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public Dictionary<string, string> Errors { get; } = new();

    public void AddError(string field, string message)
    {
        if (!Errors.ContainsKey(field))
        {
            Errors[field] = message;
        }
    }

    public string GetFormattedErrors()
    {
        return string.Join(Environment.NewLine, Errors.Select(e => $"{e.Key}: {e.Value}"));
    }
}
