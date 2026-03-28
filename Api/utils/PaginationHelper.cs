using System;
using System.Globalization;
using System.Linq;
using Api.Constants;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Api.Utilities
{
    internal static class PaginationHelper
    {
        // Valores por defecto si no se puede acceder a la configuración
        private const int DefaultMaxPageSize = 100;
        private const int DefaultPageSize = 10;
        private const int DefaultMinPage = 1;

        public static PaginationParameters GetPaginationParametersFromQueryString(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<AppSettings>? appSettings = null)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var paginationSettings = appSettings?.CurrentValue.Pagination;

            int maxPageSize = paginationSettings?.MaxPageSize ?? DefaultMaxPageSize;
            int defaultPageSize = paginationSettings?.DefaultPageSize ?? DefaultPageSize;
            int minPage = paginationSettings?.MinPage ?? DefaultMinPage;

            int page = defaultPageSize;
            int limit = defaultPageSize;

            if (int.TryParse(httpContext?.Request?.Query["page"].FirstOrDefault(), out int parsedPage))
            {
                page = parsedPage;
            }

            if (int.TryParse(httpContext?.Request?.Query["limit"].FirstOrDefault(), out int parsedLimit))
            {
                limit = parsedLimit;
            }

            // Validar límites
            page = Math.Max(minPage, page);
            limit = Math.Clamp(limit, 1, maxPageSize);

            var search = httpContext?.Request?.Query["search"].FirstOrDefault();
            var sortBy = httpContext?.Request?.Query["sortBy"].FirstOrDefault();
            var sortOrder = httpContext?.Request?.Query["sortOrder"].FirstOrDefault();

            return new PaginationParameters
            {
                Page = page,
                Limit = limit,
                Search = search,
                SortBy = sortBy,
                SortOrder = sortOrder
            };
        }
    }

    public sealed class PaginationParameters
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
    }
}
