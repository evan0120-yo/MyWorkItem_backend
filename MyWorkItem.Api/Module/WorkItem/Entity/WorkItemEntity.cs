namespace MyWorkItem.Api.Module.WorkItem.Entity;

public sealed class WorkItemEntity
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<UserWorkItemStatusEntity> UserStatuses { get; set; } = new List<UserWorkItemStatusEntity>();
}
