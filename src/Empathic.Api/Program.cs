using Empathic.Api.Models;
using Empathic.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PlatformStore>();
builder.Services.AddSingleton<HashingService>();
builder.Services.AddSingleton<IBlockchainAnchorService, DevelopmentBlockchainAnchorService>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

var creators = app.MapGroup("/api/creators");

creators.MapGet("/", async (PlatformStore store, CancellationToken ct) =>
    Results.Ok(await store.GetCreatorsAsync(ct)));

creators.MapGet("/{id:guid}", async (Guid id, PlatformStore store, CancellationToken ct) =>
{
    var creator = await store.GetCreatorAsync(id, ct);
    return creator is null ? Results.NotFound() : Results.Ok(creator);
});

creators.MapPost("/", async (CreateCreatorRequest request, PlatformStore store, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.DisplayName))
        return Results.BadRequest(new { message = "Display name is required." });

    var creator = new Creator(
        Guid.NewGuid(),
        request.DisplayName.Trim(),
        request.Institution?.Trim(),
        request.Country?.Trim(),
        request.Website?.Trim(),
        request.Roles?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct().ToArray() ?? [],
        DateTimeOffset.UtcNow);

    await store.AddCreatorAsync(creator, ct);
    return Results.Created($"/api/creators/{creator.Id}", creator);
});

var works = app.MapGroup("/api/works");

works.MapGet("/", async (PlatformStore store, CancellationToken ct) =>
    Results.Ok(await store.GetWorksAsync(ct)));

works.MapGet("/{id:guid}", async (Guid id, PlatformStore store, CancellationToken ct) =>
{
    var work = await store.GetWorkAsync(id, ct);
    return work is null ? Results.NotFound() : Results.Ok(work);
});

works.MapPost("/", async (CreateWorkRequest request, PlatformStore store, HashingService hashing, CancellationToken ct) =>
{
    if (await store.GetCreatorAsync(request.CreatorId, ct) is null)
        return Results.BadRequest(new { message = "Creator does not exist." });

    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.WorkType))
        return Results.BadRequest(new { message = "Title and work type are required." });

    var material = string.IsNullOrWhiteSpace(request.ContentFingerprintSource)
        ? $"{request.CreatorId:N}|{request.Title.Trim()}|{request.WorkType.Trim()}|{request.Description?.Trim()}|{request.ExternalUri?.Trim()}"
        : request.ContentFingerprintSource.Trim();

    var hash = hashing.Sha256(material);

    if (await store.FindByHashAsync(hash, ct) is not null)
        return Results.Conflict(new { message = "This content fingerprint is already registered.", contentHash = hash });

    var work = new CulturalWork(
        Guid.NewGuid(),
        request.CreatorId,
        request.Title.Trim(),
        request.WorkType.Trim(),
        request.Description?.Trim() ?? string.Empty,
        hash,
        request.ExternalUri?.Trim(),
        string.IsNullOrWhiteSpace(request.RightsStatement) ? "All rights reserved" : request.RightsStatement.Trim(),
        DateTimeOffset.UtcNow,
        "registered-off-chain");

    await store.AddWorkAsync(work, ct);
    return Results.Created($"/api/works/{work.Id}", work);
});

works.MapPost("/{id:guid}/anchor", async (Guid id, PlatformStore store, IBlockchainAnchorService blockchain, CancellationToken ct) =>
{
    var work = await store.GetWorkAsync(id, ct);
    if (work is null) return Results.NotFound();

    if (!string.IsNullOrWhiteSpace(work.TransactionHash))
        return Results.Ok(work);

    var result = await blockchain.AnchorAsync(work.ContentHash, work.Id, work.CreatorId, ct);
    var updated = work with
    {
        ProvenanceStatus = "anchored",
        BlockchainNetwork = result.Network,
        TransactionHash = result.TransactionHash
    };

    await store.UpdateWorkAsync(updated, ct);
    return Results.Ok(updated);
});

app.MapGet("/api/verify/{hash}", async (string hash, PlatformStore store, CancellationToken ct) =>
{
    var work = await store.FindByHashAsync(hash.Trim(), ct);
    if (work is null)
        return Results.NotFound(new { verified = false, message = "No provenance record was found for this hash." });

    var creator = await store.GetCreatorAsync(work.CreatorId, ct);
    return Results.Ok(new
    {
        verified = true,
        work,
        creator = creator is null ? null : new { creator.Id, creator.DisplayName, creator.Institution, creator.Country, creator.Roles }
    });
});

app.MapGet("/api/dashboard", async (PlatformStore store, CancellationToken ct) =>
{
    var creatorList = await store.GetCreatorsAsync(ct);
    var workList = await store.GetWorksAsync(ct);
    return Results.Ok(new
    {
        creators = creatorList.Count,
        works = workList.Count,
        anchoredWorks = workList.Count(x => x.ProvenanceStatus == "anchored"),
        pendingAnchoring = workList.Count(x => x.ProvenanceStatus != "anchored")
    });
});

app.MapFallbackToFile("index.html");
app.Run();

public sealed record CreateCreatorRequest(
    string DisplayName,
    string? Institution,
    string? Country,
    string? Website,
    string[]? Roles);

public sealed record CreateWorkRequest(
    Guid CreatorId,
    string Title,
    string WorkType,
    string? Description,
    string? ExternalUri,
    string? RightsStatement,
    string? ContentFingerprintSource);
