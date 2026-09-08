using Empathic.Application.Abstractions;
using Empathic.Application.Contracts;
using Empathic.Application.Services;
using Empathic.Infrastructure.Persistence;
using Empathic.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
builder.Services.AddSingleton(new JsonPlatformStore(dataDirectory));
builder.Services.AddSingleton<ICreatorRepository>(sp => sp.GetRequiredService<JsonPlatformStore>());
builder.Services.AddSingleton<ICulturalWorkRepository>(sp => sp.GetRequiredService<JsonPlatformStore>());
builder.Services.AddSingleton<IHashingService, Sha256HashingService>();
builder.Services.AddSingleton<IBlockchainAnchorService, DevelopmentBlockchainAnchorService>();
builder.Services.AddSingleton<CreatorService>();
builder.Services.AddSingleton<CulturalWorkService>();
builder.Services.AddSingleton<ProvenanceService>();
builder.Services.AddSingleton<DashboardService>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

var creators = app.MapGroup("/api/creators");

creators.MapGet("/", async (CreatorService service, CancellationToken ct) =>
    Results.Ok(await service.GetAllAsync(ct)));

creators.MapGet("/{id:guid}", async (Guid id, CreatorService service, CancellationToken ct) =>
{
    var creator = await service.GetAsync(id, ct);
    return creator is null ? Results.NotFound() : Results.Ok(creator);
});

creators.MapPost("/", async (CreateCreatorRequest request, CreatorService service, CancellationToken ct) =>
{
    try
    {
        var creator = await service.CreateAsync(request, ct);
        return Results.Created($"/api/creators/{creator.Id}", creator);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

var works = app.MapGroup("/api/works");

works.MapGet("/", async (CulturalWorkService service, CancellationToken ct) =>
    Results.Ok(await service.GetAllAsync(ct)));

works.MapGet("/{id:guid}", async (Guid id, CulturalWorkService service, CancellationToken ct) =>
{
    var work = await service.GetAsync(id, ct);
    return work is null ? Results.NotFound() : Results.Ok(work);
});

works.MapPost("/", async (CreateWorkRequest request, CulturalWorkService service, CancellationToken ct) =>
{
    try
    {
        var work = await service.CreateAsync(request, ct);
        return Results.Created($"/api/works/{work.Id}", work);
    }
    catch (DuplicateContentException ex)
    {
        return Results.Conflict(new { message = ex.Message, contentHash = ex.ContentHash });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

works.MapPost("/{id:guid}/anchor", async (Guid id, ProvenanceService service, CancellationToken ct) =>
{
    var work = await service.AnchorAsync(id, ct);
    return work is null ? Results.NotFound() : Results.Ok(work);
});

app.MapGet("/api/verify/{hash}", async (
    string hash,
    CulturalWorkService worksService,
    CreatorService creatorsService,
    CancellationToken ct) =>
{
    var work = await worksService.FindByHashAsync(hash, ct);
    if (work is null)
        return Results.NotFound(new { verified = false, message = "No provenance record was found for this hash." });

    var creator = await creatorsService.GetAsync(work.CreatorId, ct);
    return Results.Ok(new
    {
        verified = true,
        work,
        creator = creator is null ? null : new { creator.Id, creator.DisplayName, creator.Institution, creator.Country, creator.Roles }
    });
});

app.MapGet("/api/dashboard", async (DashboardService service, CancellationToken ct) =>
    Results.Ok(await service.GetAsync(ct)));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Empathic.Api",
    architecture = "clean-modular-monolith",
    utc = DateTimeOffset.UtcNow
}));

app.MapFallbackToFile("index.html");
app.Run();
