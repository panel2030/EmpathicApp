using System.Text.Json;
using Empathic.Application.Abstractions;
using Empathic.Domain.Entities;

namespace Empathic.Infrastructure.Persistence;

public sealed class JsonPlatformStore(string dataDirectory) : ICreatorRepository, ICulturalWorkRepository
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _dataDirectory = EnsureDirectory(dataDirectory);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public Task<IReadOnlyList<Creator>> GetAllAsync(CancellationToken ct = default) => ReadAsync<Creator>("creators.json", ct);

    async Task<IReadOnlyList<CulturalWork>> ICulturalWorkRepository.GetAllAsync(CancellationToken ct) =>
        await ReadAsync<CulturalWork>("works.json", ct);

    public async Task<Creator?> GetAsync(Guid id, CancellationToken ct = default) =>
        (await ReadAsync<Creator>("creators.json", ct)).FirstOrDefault(x => x.Id == id);

    async Task<CulturalWork?> ICulturalWorkRepository.GetAsync(Guid id, CancellationToken ct) =>
        (await ReadAsync<CulturalWork>("works.json", ct)).FirstOrDefault(x => x.Id == id);

    public async Task<CulturalWork?> FindByHashAsync(string hash, CancellationToken ct = default) =>
        (await ReadAsync<CulturalWork>("works.json", ct)).FirstOrDefault(x => x.ContentHash.Equals(hash, StringComparison.OrdinalIgnoreCase));

    public async Task AddAsync(Creator creator, CancellationToken ct = default) =>
        await AppendAsync("creators.json", creator, ct);

    async Task ICulturalWorkRepository.AddAsync(CulturalWork work, CancellationToken ct) =>
        await AppendAsync("works.json", work, ct);

    public async Task UpdateAsync(CulturalWork work, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var items = (await ReadUnsafeAsync<CulturalWork>("works.json", ct)).ToList();
            var index = items.FindIndex(x => x.Id == work.Id);
            if (index < 0) throw new KeyNotFoundException($"Cultural work {work.Id} was not found.");
            items[index] = work;
            await WriteUnsafeAsync("works.json", items, ct);
        }
        finally { _gate.Release(); }
    }

    private async Task AppendAsync<T>(string fileName, T item, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var items = (await ReadUnsafeAsync<T>(fileName, ct)).ToList();
            items.Add(item);
            await WriteUnsafeAsync(fileName, items, ct);
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

    private static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
}
