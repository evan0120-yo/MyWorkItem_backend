namespace MyWorkItem.Api.Module.WorkItem.Dto;

public sealed class ConfirmWorkItemsRequest
{
    public IReadOnlyList<Guid> WorkItemIds { get; set; } = [];
}
