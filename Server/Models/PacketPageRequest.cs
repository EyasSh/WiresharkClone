using System;
using Microsoft.AspNetCore.Mvc;
namespace Server.Models;

public class PacketPageRequest
{
    // Default page size
    public static int PacketsPerPage { get; set; } = 10;

    // Optional cap to prevent abuse
    public static int MaxPageSize { get; set; } = 100;

    // Query params
    [FromQuery(Name = "userId")]
    public string? UserId { get; set; }

    private int _pageNumber = 1;
    [FromQuery(Name = "pageNumber")]
    public int PageNumber
    {
        get => _pageNumber <= 0 ? 1 : _pageNumber;
        set => _pageNumber = value;
    }

    private int _pageSize = PacketsPerPage;
    [FromQuery(Name = "pageSize")]
    public int PageSize
    {
        get
        {
            if (_pageSize <= 0) return PacketsPerPage;
            if (_pageSize > MaxPageSize) return MaxPageSize;
            return _pageSize;
        }
        set => _pageSize = value;
    }

    // Ask server to compute total count (can be heavy on large collections)
    [FromQuery(Name = "includeTotal")]
    public bool IncludeTotal { get; set; } = true;

    // Convenience
    public int Skip => (PageNumber - 1) * PageSize;
    public int Limit => PageSize;
}