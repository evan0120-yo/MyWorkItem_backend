namespace MyWorkItem.Api.Module.WorkItem.Dto;

public sealed class ListWorkItemsResponse
{
    public IReadOnlyList<WorkItemListItemResponse> Items { get; init; } = [];
}
