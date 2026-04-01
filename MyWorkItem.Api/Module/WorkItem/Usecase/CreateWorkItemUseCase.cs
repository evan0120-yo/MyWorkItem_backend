using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Service;
using MyWorkItem.Api.Module.WorkItem.Validator;

namespace MyWorkItem.Api.Module.WorkItem.Usecase;

public sealed class CreateWorkItemUseCase(
    CurrentUserService currentUserService,
    CreateWorkItemRequestValidator validator,
    WorkItemCommandService workItemCommandService)
{
    public async Task<AdminWorkItemResponse> ExecuteAsync(
        CreateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        // 1. 驗證目前角色必須為 Admin，否則流程直接中止。
        await currentUserService.EnsureAdminAsync(cancellationToken);

        // 2. 驗證建立 request 的必要欄位是否合法。
        validator.Validate(request);

        // 3. 建立新的 Work Item 主資料並回傳結果。
        return await workItemCommandService.CreateAsync(request, cancellationToken);
    }
}
