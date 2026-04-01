namespace MyWorkItem.Api.Module.WorkItem.Dto;

public sealed class CreateWorkItemRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }
}
