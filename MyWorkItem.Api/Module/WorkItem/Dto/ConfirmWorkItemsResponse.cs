using MyWorkItem.Api.Module.WorkItem.Entity;

namespace MyWorkItem.Api.Module.WorkItem.Dto;

public sealed class ConfirmWorkItemsResponse
{
    public int ConfirmedCount { get; init; }

    public PersonalStatus Status { get; init; }
}
