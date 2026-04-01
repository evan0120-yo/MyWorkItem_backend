using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Repository;

namespace MyWorkItem.Api.Module.WorkItem.Service;

public sealed class WorkItemQueryService(
    IWorkItemRepository workItemRepository,
    IUserWorkItemStatusRepository userWorkItemStatusRepository)
{
    public async Task<ListWorkItemsResponse> GetListAsync(
        WorkItemSortDirection sortDirection,
        string userId,
        CancellationToken cancellationToken)
    {
        var workItems = await workItemRepository.ListAsync(sortDirection, cancellationToken);
        var workItemIds = workItems.Select(x => x.Id).ToList();
        var statuses = await userWorkItemStatusRepository.GetByUserAndWorkItemIdsAsync(
            userId,
            workItemIds,
            asTracking: false,
            cancellationToken);

        var statusByWorkItemId = statuses.ToDictionary(x => x.WorkItemId, x => x.Status);

        return new ListWorkItemsResponse
        {
            Items = workItems
                .Select(workItem => new WorkItemListItemResponse
                {
                    Id = workItem.Id,
                    Title = workItem.Title,
                    Status = statusByWorkItemId.GetValueOrDefault(workItem.Id, PersonalStatus.Pending),
                })
                .ToList(),
        };
    }

    public async Task<WorkItemDetailResponse> GetDetailAsync(
        Guid workItemId,
        string userId,
        CancellationToken cancellationToken)
    {
        var workItem = await workItemRepository.GetByIdAsync(workItemId, asTracking: false, cancellationToken)
            ?? throw new AppNotFoundException($"Work item '{workItemId}' was not found.");

        var userStatus = await userWorkItemStatusRepository.GetByUserAndWorkItemIdAsync(
            userId,
            workItemId,
            asTracking: false,
            cancellationToken);

        return new WorkItemDetailResponse
        {
            Id = workItem.Id,
            Title = workItem.Title,
            Description = workItem.Description,
            CreatedAt = workItem.CreatedAt,
            UpdatedAt = workItem.UpdatedAt,
            Status = userStatus?.Status ?? PersonalStatus.Pending,
        };
    }
}
