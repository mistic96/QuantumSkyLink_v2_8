using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.WaitingList
{
    /// <summary>
    /// Request model for adding user to waiting list
    /// </summary>
    public class AddToWaitingListRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        [Required]
        public string ApplicationType { get; set; } = "XL";
    }

    /// <summary>
    /// Response model for adding user to waiting list
    /// </summary>
    public class AddToWaitingListDataResponse
    {
        public bool Success { get; set; }
        public int Position { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Standard response wrapper for waiting list add operation
    /// </summary>
    public class AddToWaitingListResponse
    {
        public int Status { get; set; } = 200;
        public AddToWaitingListDataResponse Data { get; set; } = new();
    }

    /// <summary>
    /// Response model for waiting list status
    /// </summary>
    public class WaitingListStatusDataResponse
    {
        public string Email { get; set; } = string.Empty;
        public int Position { get; set; }
        public string EstimatedWaitTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; }
    }

    /// <summary>
    /// Standard response wrapper for waiting list status
    /// </summary>
    public class WaitingListStatusResponse
    {
        public int Status { get; set; } = 200;
        public WaitingListStatusDataResponse Data { get; set; } = new();
    }
}
