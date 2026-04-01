using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Service;
using MyWorkItem.Api.Module.WorkItem.Validator;

namespace MyWorkItem.Api.Module.WorkItem.Usecase;

public sealed class ConfirmWorkItemsUseCase(
    ConfirmWorkItemsRequestValidator validator,
    CurrentUserService currentUserService,
    WorkItemStatusService workItemStatusService)
{
    public async Task<ConfirmWorkItemsResponse> ExecuteAsync(
        ConfirmWorkItemsRequest request,
        CancellationToken cancellationToken)
    {
        // 1. 驗證 request 內容，並整理成唯一的 Work Item id 清單。
        var workItemIds = validator.ValidateAndGetDistinctIds(request);

        // 2. 解析目前使用者，確保只會變更自己的個人狀態。
        var currentUser = await currentUserService.GetRequiredCurrentUserAsync(cancellationToken);

        // 3. 將 request 內所有 Work Item 對目前使用者設為 Confirmed。
        return await workItemStatusService.ConfirmAsync(workItemIds, currentUser, cancellationToken);
    }
}
