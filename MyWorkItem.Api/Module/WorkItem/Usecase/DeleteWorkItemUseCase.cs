using MyWorkItem.Api.Module.WorkItem.Service;

namespace MyWorkItem.Api.Module.WorkItem.Usecase;

public sealed class DeleteWorkItemUseCase(
    CurrentUserService currentUserService,
    WorkItemCommandService workItemCommandService)
{
    public async Task ExecuteAsync(Guid workItemId, CancellationToken cancellationToken)
    {
        // 1. 驗證目前角色必須為 Admin，否則流程直接中止。
        await currentUserService.EnsureAdminAsync(cancellationToken);

        // 2. 刪除目標 Work Item 主資料，若不存在則由下層丟出 not found。
        await workItemCommandService.DeleteAsync(workItemId, cancellationToken);
    }
}
