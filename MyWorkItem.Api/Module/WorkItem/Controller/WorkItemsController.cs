using Microsoft.AspNetCore.Mvc;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Usecase;

namespace MyWorkItem.Api.Module.WorkItem.Controller;

[ApiController]
[Route("api/work-items")]
public sealed class WorkItemsController(
    ListWorkItemsUseCase listWorkItemsUseCase,
    GetWorkItemDetailUseCase getWorkItemDetailUseCase,
    ConfirmWorkItemsUseCase confirmWorkItemsUseCase,
    RevertWorkItemConfirmationUseCase revertWorkItemConfirmationUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ListWorkItemsResponse>> ListAsync(
        [FromQuery] ListWorkItemsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await listWorkItemsUseCase.ExecuteAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkItemDetailResponse>> GetDetailAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await getWorkItemDetailUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<ConfirmWorkItemsResponse>> ConfirmAsync(
        [FromBody] ConfirmWorkItemsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await confirmWorkItemsUseCase.ExecuteAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/revert-confirmation")]
    public async Task<ActionResult<RevertWorkItemConfirmationResponse>> RevertConfirmationAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await revertWorkItemConfirmationUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(response);
    }
}
