using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Dto;

public sealed class WorkItemDetailResponse
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public PersonalStatus Status { get; init; }
}
