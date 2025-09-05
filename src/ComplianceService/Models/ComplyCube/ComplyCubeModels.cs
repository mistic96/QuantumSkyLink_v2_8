namespace ComplianceService.Models.ComplyCube;

// ComplyCube Client Creation Models
public class ComplyCubeClientCreateRequest
{
    public string Type { get; set; } = "individual"; // individual, company
    public string Email { get; set; } = string.Empty;
    public PersonDetails PersonDetails { get; set; } = new();
    public string? ReferenceId { get; set; }
}

public class PersonDetails
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public Address? Address { get; set; }
}

public class Address
{
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class ComplyCubeClientResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public PersonDetails PersonDetails { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ComplyCube Check Models
public class ComplyCubeCheckCreateRequest
{
    public string Type { get; set; } = string.Empty; // document_check, face_check, address_check
    public string ClientId { get; set; } = string.Empty;
    public DocumentCheckOptions? DocumentCheck { get; set; }
    public FaceCheckOptions? FaceCheck { get; set; }
    public AddressCheckOptions? AddressCheck { get; set; }
}

public class DocumentCheckOptions
{
    public string DocumentId { get; set; } = string.Empty;
    public bool ValidateDocumentExpiry { get; set; } = true;
    public bool ValidateDocumentIssuer { get; set; } = true;
    public bool ExtractData { get; set; } = true;
}

public class FaceCheckOptions
{
    public string LivePhotoId { get; set; } = string.Empty;
    public string DocumentPhotoId { get; set; } = string.Empty;
    public bool LivenessCheck { get; set; } = true;
}

public class AddressCheckOptions
{
    public string DocumentId { get; set; } = string.Empty;
    public bool ValidateAddress { get; set; } = true;
}

public class ComplyCubeCheckResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // pending, complete, failed
    public string Result { get; set; } = string.Empty; // clear, consider, unresolved
    public CheckBreakdown Breakdown { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CheckBreakdown
{
    public DocumentBreakdown? Document { get; set; }
    public FaceBreakdown? Face { get; set; }
    public AddressBreakdown? Address { get; set; }
}

public class DocumentBreakdown
{
    public string Result { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string IssuingCountry { get; set; } = string.Empty;
    public bool IsExpired { get; set; }
    public bool IsAuthentic { get; set; }
    public ExtractedData? ExtractedData { get; set; }
}

public class FaceBreakdown
{
    public string Result { get; set; } = string.Empty;
    public decimal MatchScore { get; set; }
    public bool IsLive { get; set; }
}

public class AddressBreakdown
{
    public string Result { get; set; } = string.Empty;
    public bool IsValidAddress { get; set; }
    public string? ValidationResult { get; set; }
}

public class ExtractedData
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Nationality { get; set; }
}

// Webhook Models
public class ComplyCubeWebhookPayload
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // check.completed, check.failed, review.updated
    public WebhookData Data { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class WebhookData
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public CheckBreakdown? Breakdown { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Risk Assessment Models
public class RiskAssessmentRequest
{
    public string ClientId { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new(); // geography, transaction_patterns, document_quality
}

public class RiskAssessmentResponse
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public decimal RiskScore { get; set; } // 0.0 to 1.0
    public string RiskLevel { get; set; } = string.Empty; // low, medium, high
    public List<RiskFactor> RiskFactors { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class RiskFactor
{
    public string Type { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Description { get; set; } = string.Empty;
}
