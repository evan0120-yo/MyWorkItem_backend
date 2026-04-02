using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;

namespace MyWorkItem.Api.Module.WorkItem.Validator;

public sealed class CreateWorkItemRequestValidator
{
    private const int TitleMaxLength = 200;
    private const int DescriptionMaxLength = 2000;

    public void Validate(CreateWorkItemRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors["title"] = ["title is required."];
        }
        else if (request.Title.Trim().Length > TitleMaxLength)
        {
            errors["title"] = [$"title must be {TitleMaxLength} characters or fewer."];
        }

        if ((request.Description?.Trim().Length ?? 0) > DescriptionMaxLength)
        {
            errors["description"] =
                [$"description must be {DescriptionMaxLength} characters or fewer."];
        }

        if (errors.Count > 0)
        {
            throw new AppValidationException("Create work item request is invalid.", errors);
        }
    }
}
