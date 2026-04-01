using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Service;

namespace MyWorkItem.Api.Module.WorkItem.Usecase;

public sealed class RevertWorkItemConfirmationUseCase(
    CurrentUserService currentUserService,
    WorkItemStatusService workItemStatusService)
{
    public async Task<RevertWorkItemConfirmationResponse> ExecuteAsync(
        Guid workItemId,
        CancellationToken cancellationToken)
    {
        // 1. 解析目前使用者，確保撤銷只會影響自己的狀態。
        var currentUser = await currentUserService.GetRequiredCurrentUserAsync(cancellationToken);

        // 2. 將目前使用者對目標 Work Item 的狀態改回 Pending。
        return await workItemStatusService.RevertAsync(workItemId, currentUser.UserId, cancellationToken);
    }
}
