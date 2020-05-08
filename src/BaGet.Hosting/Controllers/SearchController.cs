using BaGet.Core;
using BaGet.Protocol.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Hosting
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly IMirrorService _mirrorService;

        public SearchController(ISearchService searchService, IMirrorService mirrorService)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _mirrorService = mirrorService;
        }

        public Task<ActionResult<SearchResponse>> SearchAsync(
            [FromQuery(Name = "q")] string query = null,
            [FromQuery]int skip = 0,
            [FromQuery]int take = 20,
            [FromQuery]bool prerelease = false,
            [FromQuery]string semVerLevel = null,

            // These are unofficial parameters
            [FromQuery]string packageType = null,
            [FromQuery]string framework = null,
            CancellationToken cancellationToken = default)
        {
            var includeSemVer2 = semVerLevel == "2.0.0";

            var cachedTask = _searchService.SearchAsync(
                query ?? string.Empty,
                skip,
                take,
                prerelease,
                includeSemVer2,
                packageType,
                framework,
                cancellationToken);

            var nugetTask = _mirrorService.SearchAsync(
                                query ?? string.Empty,
                                skip,
                                take,
                                prerelease,
                                includeSemVer2,
                                packageType,
                                framework,
                                cancellationToken);
            return Merge(cachedTask, nugetTask);
        }

        private async Task<ActionResult<SearchResponse>> Merge(Task<SearchResponse> cachedTask, Task<SearchResponse> nugetTask)
        {
            var nugetResponse = await nugetTask.ConfigureAwait(false);
            var cacheResponse = await cachedTask.ConfigureAwait(false);
            var nugetResponseData = nugetResponse?.Data ?? new List<SearchResult>();
            var cacheResponseData = cacheResponse.Data;

            // VS expects 26 packages, but shows only 25, so make sure we deliver only 26
            // additional packages would disappear
            var diff = Math.Abs(nugetResponseData.Count - cacheResponseData.Count);
            nugetResponseData = nugetResponseData.Take(diff).ToList();

            var searchResults = cacheResponseData.Union(nugetResponseData) // combine both lists - cached packages are preferred over nuget packages to show
                .GroupBy(o => o.PackageId) // group by package id
                .OrderBy(o => o.Count())
                .Select(o => o.OrderByDescending(p => p.TotalDownloads).First()); // take the package with most downloads

            var searchResponse = new SearchResponse { Context = null, Data = searchResults.ToList() };
            searchResponse.TotalHits = searchResponse.Data.Count;
            return searchResponse;
        }

        public async Task<ActionResult<AutocompleteResponse>> AutocompleteAsync(
            [FromQuery(Name = "q")] string query = null,
            CancellationToken cancellationToken = default)
        {
            // TODO: Add other autocomplete parameters
            // TODO: Support versions autocomplete.
            // See: https://github.com/loic-sharma/BaGet/issues/291
            return await _searchService.AutocompleteAsync(
                query,
                cancellationToken: cancellationToken);
        }

        public async Task<ActionResult<DependentsResponse>> DependentsAsync(
            [FromQuery] string packageId,
            CancellationToken cancellationToken = default)
        {
            // TODO: Add other dependents parameters.
            return await _searchService.FindDependentsAsync(
                packageId,
                cancellationToken: cancellationToken);
        }
    }
}
