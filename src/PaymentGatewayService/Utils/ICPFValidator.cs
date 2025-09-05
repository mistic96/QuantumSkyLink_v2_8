namespace PaymentGatewayService.Utils;

/// <summary>
/// Interface for Brazilian CPF/CNPJ document validation
/// </summary>
public interface ICPFValidator
{
    /// <summary>
    /// Validates if a CPF or CNPJ document number is valid
    /// </summary>
    /// <param name="document">The CPF or CNPJ document number</param>
    /// <returns>True if the document is valid, false otherwise</returns>
    bool IsValid(string document);

    /// <summary>
    /// Validates if a CPF document number is valid
    /// </summary>
    /// <param name="cpf">The CPF document number</param>
    /// <returns>True if the CPF is valid, false otherwise</returns>
    bool IsValidCPF(string cpf);

    /// <summary>
    /// Validates if a CNPJ document number is valid
    /// </summary>
    /// <param name="cnpj">The CNPJ document number</param>
    /// <returns>True if the CNPJ is valid, false otherwise</returns>
    bool IsValidCNPJ(string cnpj);

    /// <summary>
    /// Formats a CPF or CNPJ document number
    /// </summary>
    /// <param name="document">The unformatted document number</param>
    /// <returns>Formatted document string or null if invalid</returns>
    string? Format(string document);

    /// <summary>
    /// Removes formatting from a CPF or CNPJ document
    /// </summary>
    /// <param name="document">The formatted document number</param>
    /// <returns>Unformatted document string</returns>
    string Clean(string document);
}