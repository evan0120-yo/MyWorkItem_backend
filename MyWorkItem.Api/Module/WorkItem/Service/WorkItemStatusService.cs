using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Repository;

namespace MyWorkItem.Api.Module.WorkItem.Service;

public sealed class WorkItemStatusService(
    IWorkItemRepository workItemRepository,
    IUserWorkItemStatusRepository userWorkItemStatusRepository,
    IUserRepository userRepository)
{
    public async Task<ConfirmWorkItemsResponse> ConfirmAsync(
        IReadOnlyCollection<Guid> workItemIds,
        CurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var workItems = await workItemRepository.GetByIdsAsync(workItemIds, cancellationToken);

        if (workItems.Count != workItemIds.Count)
        {
            throw new AppNotFoundException("一筆或多筆工作項目不存在。");
        }

        await EnsureUserRecordAsync(currentUser, cancellationToken);

        var existingStatuses = await userWorkItemStatusRepository.GetByUserAndWorkItemIdsAsync(
            currentUser.UserId,
            workItemIds,
            asTracking: true,
            cancellationToken);

        var statusByWorkItemId = existingStatuses.ToDictionary(x => x.WorkItemId, x => x);
        var now = DateTime.UtcNow;
        var newStatuses = new List<UserWorkItemStatusEntity>();

        foreach (var workItemId in workItemIds)
        {
            if (statusByWorkItemId.TryGetValue(workItemId, out var existingStatus))
            {
                existingStatus.Status = PersonalStatus.Confirmed;
                existingStatus.UpdatedAt = now;
                continue;
            }

            newStatuses.Add(
                new UserWorkItemStatusEntity
                {
                    UserId = currentUser.UserId,
                    WorkItemId = workItemId,
                    Status = PersonalStatus.Confirmed,
                    UpdatedAt = now,
                });
        }

        if (newStatuses.Count > 0)
        {
            await userWorkItemStatusRepository.AddRangeAsync(newStatuses, cancellationToken);
        }

        await userWorkItemStatusRepository.SaveChangesAsync(cancellationToken);

        return new ConfirmWorkItemsResponse
        {
            ConfirmedCount = workItemIds.Count,
            Status = PersonalStatus.Confirmed,
        };
    }

    public async Task<RevertWorkItemConfirmationResponse> RevertAsync(
        Guid workItemId,
        string userId,
        CancellationToken cancellationToken)
    {
        var workItem = await workItemRepository.GetByIdAsync(workItemId, asTracking: false, cancellationToken);

        if (workItem is null)
        {
            throw new AppNotFoundException($"找不到工作項目 '{workItemId}'。");
        }

        var existingStatus = await userWorkItemStatusRepository.GetByUserAndWorkItemIdAsync(
            userId,
            workItemId,
            asTracking: true,
            cancellationToken);

        if (existingStatus is not null)
        {
            existingStatus.Status = PersonalStatus.Pending;
            existingStatus.UpdatedAt = DateTime.UtcNow;
            await userWorkItemStatusRepository.SaveChangesAsync(cancellationToken);
        }

        return new RevertWorkItemConfirmationResponse
        {
            WorkItemId = workItemId,
            Status = PersonalStatus.Pending,
        };
    }

    private async Task EnsureUserRecordAsync(CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var persistedUser = await userRepository.GetByIdAsync(currentUser.UserId, cancellationToken);

        if (persistedUser is null)
        {
            await userRepository.AddAsync(
                new UserEntity
                {
                    Id = currentUser.UserId,
                    UserName = currentUser.UserName,
                    DisplayName = currentUser.DisplayName,
                },
                cancellationToken);
            return;
        }

        if (persistedUser.UserName != currentUser.UserName)
        {
            persistedUser.UserName = currentUser.UserName;
        }

        if (persistedUser.DisplayName != currentUser.DisplayName)
        {
            persistedUser.DisplayName = currentUser.DisplayName;
        }
    }
}
