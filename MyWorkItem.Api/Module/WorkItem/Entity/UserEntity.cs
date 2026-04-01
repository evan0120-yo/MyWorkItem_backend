namespace MyWorkItem.Api.Module.WorkItem.Entity;

public sealed class UserEntity
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public ICollection<UserWorkItemStatusEntity> WorkItemStatuses { get; set; } = new List<UserWorkItemStatusEntity>();
}
