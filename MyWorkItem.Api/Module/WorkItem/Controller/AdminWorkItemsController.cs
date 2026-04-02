using Microsoft.AspNetCore.Mvc;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Usecase;

namespace MyWorkItem.Api.Module.WorkItem.Controller;

[ApiController]
[Route("api/admin/work-items")]
public sealed class AdminWorkItemsController(
    CreateWorkItemUseCase createWorkItemUseCase,
    UpdateWorkItemUseCase updateWorkItemUseCase,
    DeleteWorkItemUseCase deleteWorkItemUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AdminWorkItemResponse>> CreateAsync(
        [FromBody] CreateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createWorkItemUseCase.ExecuteAsync(request, cancellationToken);
        return Created($"/api/work-items/{response.Id}", response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminWorkItemResponse>> UpdateAsync(
        Guid id,
        [FromBody] UpdateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        var response = await updateWorkItemUseCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await deleteWorkItemUseCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
