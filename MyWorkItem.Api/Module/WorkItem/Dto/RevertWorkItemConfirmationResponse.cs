using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Dto;

public sealed class RevertWorkItemConfirmationResponse
{
    public Guid WorkItemId { get; init; }

    public PersonalStatus Status { get; init; }
}
