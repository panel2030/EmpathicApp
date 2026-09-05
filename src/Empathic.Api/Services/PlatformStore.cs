using System.Text.Json;
using Empathic.Api.Models;

namespace Empathic.Api.Services;

public sealed class PlatformStore
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _dataDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public PlatformStore(IWebHostEnvironment environment)
    {
        _dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(_dataDirectory);
    }

    public async Task<IReadOnlyList<Creator>> GetCreatorsAsync(CancellationToken ct = default) =>
        await ReadAsync<Creator>("creators.json", ct);

    public async Task<Creator?> GetCreatorAsync(Guid id, CancellationToken ct = default) =>
        (await GetCreatorsAsync(ct)).FirstOrDefault(x => x.Id == id);

    public async Task<Creator> AddCreatorAsync(Creator creator, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var items = (await ReadUnsafeAsync<Creator>("creators.json", ct)).ToList();
            items.Add(creator);
            await WriteUnsafeAsync("creators.json", items, ct);
            return creator;
        }
        finally { _gate.Release(); }
    }

    public async Task<IReadOnlyList<CulturalWork>> GetWorksAsync(CancellationToken ct = default) =>
        await ReadAsync<CulturalWork>("works.json", ct);

    public async Task<CulturalWork?> GetWorkAsync(Guid id, CancellationToken ct = default) =>
        (await GetWorksAsync(ct)).FirstOrDefault(x => x.Id == id);

    public async Task<CulturalWork?> FindByHashAsync(string hash, CancellationToken ct = default) =>
        (await GetWorksAsync(ct)).FirstOrDefault(x => x.ContentHash.Equals(hash, StringComparison.OrdinalIgnoreCase));

    public async Task<CulturalWork> AddWorkAsync(CulturalWork work, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var items = (await ReadUnsafeAsync<CulturalWork>("works.json", ct)).ToList();
            items.Add(work);
            await WriteUnsafeAsync("works.json", items, ct);
            return work;
        }
        finally { _gate.Release(); }
    }

    public async Task<CulturalWork?> UpdateWorkAsync(CulturalWork work, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var items = (await ReadUnsafeAsync<CulturalWork>("works.json", ct)).ToList();
            var index = items.FindIndex(x => x.Id == work.Id);
            if (index < 0) return null;
            items[index] = work;
            await WriteUnsafeAsync("works.json", items, ct);
            return work;
        }
        finally { _gate.Release(); }
    }

    private async Task<IReadOnlyList<T>> ReadAsync<T>(string fileName, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try { return await ReadUnsafeAsync<T>(fileName, ct); }
        finally { _gate.Release(); }
    }

    private async Task<IReadOnlyList<T>> ReadUnsafeAsync<T>(string fileName, CancellationToken ct)
    {
        var path = Path.Combine(_dataDirectory, fileName);
        if (!File.Exists(path)) return Array.Empty<T>();
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions, ct) ?? [];
    }

    private async Task WriteUnsafeAsync<T>(string fileName, IEnumerable<T> items, CancellationToken ct)
    {
        var path = Path.Combine(_dataDirectory, fileName);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, items, _jsonOptions, ct);
    }
}
