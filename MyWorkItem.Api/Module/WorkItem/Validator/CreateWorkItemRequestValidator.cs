using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;

namespace MyWorkItem.Api.Module.WorkItem.Validator;

public sealed class CreateWorkItemRequestValidator
{
    public void Validate(CreateWorkItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new AppValidationException(
                "Create work item request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["title"] = ["title is required."],
                });
        }
    }
}
