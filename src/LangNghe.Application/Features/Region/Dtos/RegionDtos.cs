using System;
using System.Collections.Generic;
using LangNghe.Application.Common;

namespace LangNghe.Application.Features.Region.Dtos;

public record RegionDto
{
    public int Id { get; init; }
    public required string RegionCode { get; init; }
    public required string Name { get; init; }
    public string? ShortName { get; init; }
    public byte LevelType { get; init; }
    public string? RegionType { get; init; }
    public int? ParentId { get; init; }
    public string? ParentName { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record RegionTreeDto
{
    public int Id { get; init; }
    public required string RegionCode { get; init; }
    public required string Name { get; init; }
    public byte LevelType { get; init; }
    public List<RegionTreeDto> Children { get; init; } = new();
}

public record CreateRegionDto
{
    public required string RegionCode { get; init; }
    public required string Name { get; init; }
    public string? ShortName { get; init; }
    public byte LevelType { get; init; }
    public string? RegionType { get; init; }
    public int? ParentId { get; init; }
    public int? SortOrder { get; init; }
}

public record UpdateRegionDto
{
    public required string RegionCode { get; init; }
    public required string Name { get; init; }
    public string? ShortName { get; init; }
    public byte LevelType { get; init; }
    public string? RegionType { get; init; }
    public int? ParentId { get; init; }
    public int? SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record RegionFilterDto : PageRequest
{
    public string? Keyword { get; init; }
    public byte? LevelType { get; init; }
    public int? ParentId { get; init; }
    public bool? IsActive { get; init; }
}
