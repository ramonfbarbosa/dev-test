using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Models;

public class PagedList<T>
{
    public IReadOnlyList<T> Values { get; }
    public int CurrentPage { get; }
    public int TotalPages { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    private PagedList(IReadOnlyList<T> values, int totalCount, int currentPage, int pageSize)
    {
        Values = values;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public static PagedList<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        var items = source.ToList();
        var totalCount = items.Count;
        var paged = items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedList<T>(paged, totalCount, page, pageSize);
    }

    public static PagedList<T> Create(IReadOnlyList<T> pagedItems, int totalCount, int page, int pageSize)
    {
        return new PagedList<T>(pagedItems, totalCount, page, pageSize);
    }
}
