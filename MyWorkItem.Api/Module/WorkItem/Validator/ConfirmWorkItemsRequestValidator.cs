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
                "Confirm work items request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["workItemIds"] = ["workItemIds is required."],
                });
        }

        if (request.WorkItemIds.Any(x => x == Guid.Empty))
        {
            throw new AppValidationException(
                "Confirm work items request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["workItemIds"] = ["workItemIds must not contain empty ids."],
                });
        }

        var workItemIds = request.WorkItemIds
            .Distinct()
            .ToList();

        if (workItemIds.Count == 0)
        {
            throw new AppValidationException(
                "Confirm work items request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["workItemIds"] = ["workItemIds must contain at least one valid id."],
                });
        }

        return workItemIds;
    }
}
