using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Dto;

public sealed class WorkItemListItemResponse
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public PersonalStatus Status { get; init; }
}
