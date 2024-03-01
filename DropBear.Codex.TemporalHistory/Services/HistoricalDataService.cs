using DropBear.Codex.TemporalHistory.DataAccess;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;

namespace DropBear.Codex.TemporalHistory.Services;

public class HistoricalDataService : IHistoricalDataService
{
    private readonly TemporalDbContext _context;

    public HistoricalDataService(TemporalDbContext context)
    {
        _context = context;
    }

    public async Task<List<HistoricalRecord>> GetHistoryForRecordAsync<T>(int id) where T : class, ITemporal
    {
        // Combine ChangeLog entries with temporal table data
        // Implementation depends on your specific database schema and needs
    }
}