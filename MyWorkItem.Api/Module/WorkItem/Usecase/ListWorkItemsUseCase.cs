using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Service;
using MyWorkItem.Api.Module.WorkItem.Validator;

namespace MyWorkItem.Api.Module.WorkItem.Usecase;

public sealed class ListWorkItemsUseCase(
    ListWorkItemsRequestValidator validator,
    CurrentUserService currentUserService,
    WorkItemQueryService workItemQueryService)
{
    public async Task<ListWorkItemsResponse> ExecuteAsync(
        ListWorkItemsRequest request,
        CancellationToken cancellationToken)
    {
        // 1. 驗證列表排序參數，並取得內部排序方向。
        var sortDirection = validator.ValidateAndGetSortDirection(request);

        // 2. 解析目前使用者，確保後續查詢有固定的使用者視角。
        var currentUser = await currentUserService.GetRequiredCurrentUserAsync(cancellationToken);

        // 3. 依目前使用者視角組裝 Work Item 列表結果。
        return await workItemQueryService.GetListAsync(sortDirection, currentUser.UserId, cancellationToken);
    }
}
