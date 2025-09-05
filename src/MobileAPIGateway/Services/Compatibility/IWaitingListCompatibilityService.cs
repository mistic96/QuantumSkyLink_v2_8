using MobileAPIGateway.Models.Compatibility.WaitingList;

namespace MobileAPIGateway.Services.Compatibility
{
    /// <summary>
    /// Service interface for waiting list compatibility operations
    /// </summary>
    public interface IWaitingListCompatibilityService
    {
        /// <summary>
        /// Add user to waiting list
        /// </summary>
        Task<AddToWaitingListDataResponse> AddToWaitingListAsync(
            AddToWaitingListRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get waiting list status for user
        /// </summary>
        Task<WaitingListStatusDataResponse> GetWaitingListStatusAsync(
            string email,
            string applicationType,
            CancellationToken cancellationToken = default);
    }
}
