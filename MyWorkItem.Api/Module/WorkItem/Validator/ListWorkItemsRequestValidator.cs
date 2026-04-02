using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;

namespace MyWorkItem.Api.Module.WorkItem.Validator;

public sealed class ListWorkItemsRequestValidator
{
    public WorkItemSortDirection ValidateAndGetSortDirection(ListWorkItemsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SortDirection))
        {
            return WorkItemSortDirection.Desc;
        }

        return request.SortDirection.Trim().ToLowerInvariant() switch
        {
            "asc" => WorkItemSortDirection.Asc,
            "desc" => WorkItemSortDirection.Desc,
            _ => throw new AppValidationException(
                "查詢工作項目清單的請求資料無效。",
                new Dictionary<string, string[]>
                {
                    ["sortDirection"] = ["sortDirection 只能是 'asc' 或 'desc'。"],
                }),
        };
    }
}
