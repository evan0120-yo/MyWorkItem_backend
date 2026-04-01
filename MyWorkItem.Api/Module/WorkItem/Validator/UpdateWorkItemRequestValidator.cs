using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;

namespace MyWorkItem.Api.Module.WorkItem.Validator;

public sealed class UpdateWorkItemRequestValidator
{
    public void Validate(UpdateWorkItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new AppValidationException(
                "Update work item request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["title"] = ["title is required."],
                });
        }
    }
}
