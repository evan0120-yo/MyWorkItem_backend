using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Service;

namespace MyWorkItem.Api.Module.WorkItem.Usecase;

public sealed class GetWorkItemDetailUseCase(
    CurrentUserService currentUserService,
    WorkItemQueryService workItemQueryService)
{
    public async Task<WorkItemDetailResponse> ExecuteAsync(
        Guid workItemId,
        CancellationToken cancellationToken)
    {
        // 1. 解析目前使用者，確保 detail 狀態以個人視角回傳。
        var currentUser = await currentUserService.GetRequiredCurrentUserAsync(cancellationToken);

        // 2. 讀取目標 Work Item detail，若不存在則直接由下層丟出 not found。
        return await workItemQueryService.GetDetailAsync(workItemId, currentUser.UserId, cancellationToken);
    }
}
