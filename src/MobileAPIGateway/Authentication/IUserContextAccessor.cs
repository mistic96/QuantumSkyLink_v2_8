using MobileAPIGateway.Models;

namespace MobileAPIGateway.Authentication
{
    /// <summary>
    /// Interface for accessing the current user context
    /// </summary>
    public interface IUserContextAccessor
    {
        /// <summary>
        /// Gets the current user context
        /// </summary>
        /// <returns>The user context</returns>
        UserContext GetUserContext();
    }
}
