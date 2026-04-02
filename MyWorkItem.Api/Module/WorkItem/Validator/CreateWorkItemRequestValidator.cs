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
            errors["title"] = ["請輸入標題。"];
        }
        else if (request.Title.Trim().Length > TitleMaxLength)
        {
            errors["title"] = [$"標題長度不可超過 {TitleMaxLength} 個字元。"];
        }

        if ((request.Description?.Trim().Length ?? 0) > DescriptionMaxLength)
        {
            errors["description"] =
                [$"描述長度不可超過 {DescriptionMaxLength} 個字元。"];
        }

        if (errors.Count > 0)
        {
            throw new AppValidationException("新增工作項目請求資料無效。", errors);
        }
    }
}
