using System.Text.RegularExpressions;

namespace PaymentGatewayService.Utils;

/// <summary>
/// Implementation of Brazilian CPF/CNPJ document validator
/// </summary>
public class CPFValidator : ICPFValidator
{
    private static readonly Regex CleanRegex = new Regex(@"[^\d]", RegexOptions.Compiled);

    /// <summary>
    /// Validates if a CPF or CNPJ document number is valid
    /// </summary>
    public bool IsValid(string document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return false;

        var cleanDocument = Clean(document);

        return cleanDocument.Length switch
        {
            11 => IsValidCPF(cleanDocument),
            14 => IsValidCNPJ(cleanDocument),
            _ => false
        };
    }

    /// <summary>
    /// Validates if a CPF document number is valid
    /// </summary>
    public bool IsValidCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = Clean(cpf);

        if (cpf.Length != 11)
            return false;

        // Check for known invalid CPFs (all same digits)
        if (IsAllSameDigits(cpf))
            return false;

        // Validate first check digit
        var sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (cpf[i] - '0') * (10 - i);
        }

        var remainder = sum % 11;
        var checkDigit1 = remainder < 2 ? 0 : 11 - remainder;

        if (checkDigit1 != cpf[9] - '0')
            return false;

        // Validate second check digit
        sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += (cpf[i] - '0') * (11 - i);
        }

        remainder = sum % 11;
        var checkDigit2 = remainder < 2 ? 0 : 11 - remainder;

        return checkDigit2 == cpf[10] - '0';
    }

    /// <summary>
    /// Validates if a CNPJ document number is valid
    /// </summary>
    public bool IsValidCNPJ(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        cnpj = Clean(cnpj);

        if (cnpj.Length != 14)
            return false;

        // Check for known invalid CNPJs (all same digits)
        if (IsAllSameDigits(cnpj))
            return false;

        // CNPJ validation weights
        int[] firstWeights = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] secondWeights = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        // Validate first check digit
        var sum = 0;
        for (int i = 0; i < 12; i++)
        {
            sum += (cnpj[i] - '0') * firstWeights[i];
        }

        var remainder = sum % 11;
        var checkDigit1 = remainder < 2 ? 0 : 11 - remainder;

        if (checkDigit1 != cnpj[12] - '0')
            return false;

        // Validate second check digit
        sum = 0;
        for (int i = 0; i < 13; i++)
        {
            sum += (cnpj[i] - '0') * secondWeights[i];
        }

        remainder = sum % 11;
        var checkDigit2 = remainder < 2 ? 0 : 11 - remainder;

        return checkDigit2 == cnpj[13] - '0';
    }

    /// <summary>
    /// Formats a CPF or CNPJ document number
    /// </summary>
    public string? Format(string document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return null;

        var cleanDocument = Clean(document);

        if (!IsValid(cleanDocument))
            return null;

        return cleanDocument.Length switch
        {
            11 => FormatCPF(cleanDocument),
            14 => FormatCNPJ(cleanDocument),
            _ => null
        };
    }

    /// <summary>
    /// Removes formatting from a CPF or CNPJ document
    /// </summary>
    public string Clean(string document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return string.Empty;

        return CleanRegex.Replace(document, string.Empty);
    }

    #region Private Helper Methods

    private static bool IsAllSameDigits(string document)
    {
        if (string.IsNullOrEmpty(document))
            return true;

        var firstChar = document[0];
        return document.All(c => c == firstChar);
    }

    private static string FormatCPF(string cpf)
    {
        // Format: XXX.XXX.XXX-XX
        return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
    }

    private static string FormatCNPJ(string cnpj)
    {
        // Format: XX.XXX.XXX/XXXX-XX
        return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
    }

    #endregion
}