using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;

namespace MyWorkItem.Api.Module.WorkItem.Validator;

public sealed class ConfirmWorkItemsRequestValidator
{
    public IReadOnlyList<Guid> ValidateAndGetDistinctIds(ConfirmWorkItemsRequest request)
    {
        if (request.WorkItemIds is null)
        {
            throw new AppValidationException(
                "確認工作項目請求資料無效。",
                new Dictionary<string, string[]>
                {
                    ["workItemIds"] = ["workItemIds 為必填。"],
                });
        }

        if (request.WorkItemIds.Any(x => x == Guid.Empty))
        {
            throw new AppValidationException(
                "確認工作項目請求資料無效。",
                new Dictionary<string, string[]>
                {
                    ["workItemIds"] = ["workItemIds 不可包含空白識別碼。"],
                });
        }

        var workItemIds = request.WorkItemIds
            .Distinct()
            .ToList();

        if (workItemIds.Count == 0)
        {
            throw new AppValidationException(
                "確認工作項目請求資料無效。",
                new Dictionary<string, string[]>
                {
                    ["workItemIds"] = ["workItemIds 至少要有一筆有效識別碼。"],
                });
        }

        return workItemIds;
    }
}
