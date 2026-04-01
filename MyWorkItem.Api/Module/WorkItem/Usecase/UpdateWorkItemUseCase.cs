using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Service;
using MyWorkItem.Api.Module.WorkItem.Validator;

namespace MyWorkItem.Api.Module.WorkItem.Usecase;

public sealed class UpdateWorkItemUseCase(
    CurrentUserService currentUserService,
    UpdateWorkItemRequestValidator validator,
    WorkItemCommandService workItemCommandService)
{
    public async Task<AdminWorkItemResponse> ExecuteAsync(
        Guid workItemId,
        UpdateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        // 1. 驗證目前角色必須為 Admin，否則流程直接中止。
        await currentUserService.EnsureAdminAsync(cancellationToken);

        // 2. 驗證更新 request 的必要欄位是否合法。
        validator.Validate(request);

        // 3. 更新目標 Work Item 主資料並回傳結果。
        return await workItemCommandService.UpdateAsync(workItemId, request, cancellationToken);
    }
}
