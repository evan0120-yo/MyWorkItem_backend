namespace MyWorkItem.Api.Module.WorkItem.Entity;

public sealed class UserWorkItemStatusEntity
{
    public string UserId { get; set; } = string.Empty;

    public Guid WorkItemId { get; set; }

    public PersonalStatus Status { get; set; }

    public DateTime UpdatedAt { get; set; }

    public UserEntity? User { get; set; }

    public WorkItemEntity? WorkItem { get; set; }
}
