using BaGet.Protocol.Models;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Core
{
    /// <summary>
    /// Indexes packages from an external source.
    /// </summary>
    public interface IMirrorService
    {
        /// <summary>
        /// Attempt to find a package's versions using mirroring. This will merge
        /// results from the configured upstream source with the locally indexed packages.
        /// </summary>
        /// <param name="id">The package's id to lookup</param>
        /// <param name="cancellationToken">The token to cancel the lookup</param>
        /// <returns>
        /// The package's versions, or null if the package cannot be found on the
        /// configured upstream source. This includes unlisted versions.
        /// </returns>
        Task<IReadOnlyList<NuGetVersion>> FindPackageVersionsOrNullAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Attempt to find a package's metadata using mirroring. This will merge
        /// results from the configured upstream source with the locally indexed packages.
        /// </summary>
        /// <param name="id">The package's id to lookup</param>
        /// <param name="cancellationToken">The token to cancel the lookup</param>
        /// <returns>
        /// The package's metadata, or null if the package cannot be found on the configured
        /// upstream source.
        /// </returns>
        Task<IReadOnlyList<Package>> FindPackagesOrNullAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// If the package is unknown, attempt to index it from an upstream source.
        /// </summary>
        /// <param name="id">The package's id</param>
        /// <param name="version">The package's version</param>
        /// <param name="cancellationToken">The token to cancel the mirroring</param>
        /// <returns>A task that completes when the package has been mirrored.</returns>
        Task MirrorAsync(string id, NuGetVersion version, CancellationToken cancellationToken);

        /// <summary>
        /// Perform a search query.
        /// See: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource#search-for-packages
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="skip">How many results to skip.</param>
        /// <param name="take">How many results to return.</param>
        /// <param name="includePrerelease">Whether pre-release packages should be returned.</param>
        /// <param name="includeSemVer2">Whether packages that require SemVer 2.0.0 compatibility should be returned.</param>
        /// <param name="packageType">The type of packages that should be returned.</param>
        /// <param name="framework">The Target Framework that results should be compatible.</param>
        /// <param name="cancellationToken">A token to cancel the task.</param>
        /// <returns>The search response.</returns>
        Task<SearchResponse> SearchAsync(
            string query = null,
            int skip = 0,
            int take = 20,
            bool includePrerelease = true,
            bool includeSemVer2 = true,
            string packageType = null,
            string framework = null,
            CancellationToken cancellationToken = default);
    }
}
