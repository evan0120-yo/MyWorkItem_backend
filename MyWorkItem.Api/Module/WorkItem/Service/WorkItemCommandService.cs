using MyWorkItem.Api.Infrastructure.Exceptions;
using MyWorkItem.Api.Module.WorkItem.Dto;
using MyWorkItem.Api.Module.WorkItem.Entity;
using MyWorkItem.Api.Module.WorkItem.Repository;

namespace MyWorkItem.Api.Module.WorkItem.Service;

public sealed class WorkItemCommandService(IWorkItemRepository workItemRepository)
{
    public async Task<AdminWorkItemResponse> CreateAsync(
        CreateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var workItem = new WorkItemEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title!.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await workItemRepository.AddAsync(workItem, cancellationToken);
        await workItemRepository.SaveChangesAsync(cancellationToken);

        return MapAdminResponse(workItem);
    }

    public async Task<AdminWorkItemResponse> UpdateAsync(
        Guid workItemId,
        UpdateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        var workItem = await workItemRepository.GetByIdAsync(workItemId, asTracking: true, cancellationToken)
            ?? throw new AppNotFoundException($"Work item '{workItemId}' was not found.");

        workItem.Title = request.Title!.Trim();
        workItem.Description = request.Description?.Trim() ?? string.Empty;
        workItem.UpdatedAt = DateTime.UtcNow;

        await workItemRepository.SaveChangesAsync(cancellationToken);

        return MapAdminResponse(workItem);
    }

    public async Task DeleteAsync(Guid workItemId, CancellationToken cancellationToken)
    {
        var workItem = await workItemRepository.GetByIdAsync(workItemId, asTracking: true, cancellationToken)
            ?? throw new AppNotFoundException($"Work item '{workItemId}' was not found.");

        workItemRepository.Remove(workItem);
        await workItemRepository.SaveChangesAsync(cancellationToken);
    }

    private static AdminWorkItemResponse MapAdminResponse(WorkItemEntity workItem)
    {
        return new AdminWorkItemResponse
        {
            Id = workItem.Id,
            Title = workItem.Title,
            Description = workItem.Description,
            CreatedAt = workItem.CreatedAt,
            UpdatedAt = workItem.UpdatedAt,
        };
    }
}
