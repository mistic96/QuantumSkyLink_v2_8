using System.ComponentModel.DataAnnotations;

namespace InfrastructureService.Models.Requests
{
    /// <summary>
    /// Request to generate a blockchain address for a service.
    /// </summary>
    public class GenerateAddressRequest
    {
        /// <summary>
        /// Gets or sets the service name for which to generate the address.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blockchain network type (MultiChain, Ethereum).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional metadata for the address generation.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }

    /// <summary>
    /// Request to generate blockchain addresses for multiple services.
    /// </summary>
    public class BulkGenerateAddressRequest
    {
        /// <summary>
        /// Gets or sets the list of service names for which to generate addresses.
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<string> ServiceNames { get; set; } = new();

        /// <summary>
        /// Gets or sets the blockchain network type (MultiChain, Ethereum).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional metadata for the address generation.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }

    /// <summary>
    /// Request to validate a blockchain address.
    /// </summary>
    public class ValidateAddressRequest
    {
        /// <summary>
        /// Gets or sets the blockchain address to validate.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blockchain network type (MultiChain, Ethereum).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NetworkType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to get address information.
    /// </summary>
    public class GetAddressInfoRequest
    {
        /// <summary>
        /// Gets or sets the blockchain address to query.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blockchain network type (MultiChain, Ethereum).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NetworkType { get; set; } = string.Empty;
    }
}
