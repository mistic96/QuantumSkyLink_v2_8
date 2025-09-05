namespace QuantumLedger.Models.Validation;

/// <summary>
/// Base class for validation results
/// </summary>
public abstract class ValidationResultBase
{
    /// <summary>
    /// Gets or sets whether the validation was successful
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the validation message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets any validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
