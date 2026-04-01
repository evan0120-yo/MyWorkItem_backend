using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MyWorkItem.Api.Auth;
using MyWorkItem.Api.Infrastructure;
using MyWorkItem.Api.Module.WorkItem.Repository;
using MyWorkItem.Api.Module.WorkItem.Service;
using MyWorkItem.Api.Module.WorkItem.Usecase;
using MyWorkItem.Api.Module.WorkItem.Validator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MyWorkItemDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' is required.");

    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
builder.Services.AddScoped<IUserWorkItemStatusRepository, UserWorkItemStatusRepository>();

builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<WorkItemQueryService>();
builder.Services.AddScoped<WorkItemCommandService>();
builder.Services.AddScoped<WorkItemStatusService>();

builder.Services.AddScoped<ListWorkItemsRequestValidator>();
builder.Services.AddScoped<ConfirmWorkItemsRequestValidator>();
builder.Services.AddScoped<CreateWorkItemRequestValidator>();
builder.Services.AddScoped<UpdateWorkItemRequestValidator>();

builder.Services.AddScoped<ListWorkItemsUseCase>();
builder.Services.AddScoped<GetWorkItemDetailUseCase>();
builder.Services.AddScoped<ConfirmWorkItemsUseCase>();
builder.Services.AddScoped<RevertWorkItemConfirmationUseCase>();
builder.Services.AddScoped<CreateWorkItemUseCase>();
builder.Services.AddScoped<UpdateWorkItemUseCase>();
builder.Services.AddScoped<DeleteWorkItemUseCase>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.InitializeDatabaseAsync();

app.Run();
