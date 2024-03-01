using DropBear.Codex.TemporalHistory.DataAccess;
using DropBear.Codex.TemporalHistory.Interfaces;

namespace DropBear.Codex.TemporalHistory.Services;

public class RollbackService : IRollbackService
{
    private readonly TemporalDbContext _context;

    public RollbackService(TemporalDbContext context)
    {
        _context = context;
    }

    public async Task<bool> RollbackRecordAsync<T>(int recordId, DateTime toDateTime) where T : class, ITemporal
    {
        // Logic to revert the record state
    }
}