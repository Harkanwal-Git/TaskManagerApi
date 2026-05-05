using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _dbContext;
    public RefreshTokenRepository(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
    }
    public async Task AddRefreshToken(RefreshToken token, CancellationToken ct)
    {
        _dbContext.RefreshTokens.Add(token);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<RefreshToken?> GetRefreshToken(string token, CancellationToken ct)
    {
        RefreshToken? retreivedToken = await _dbContext.RefreshTokens.Include(r => r.User).ThenInclude(u => u.Roles).FirstOrDefaultAsync(r => r.Token == token, ct);

        return retreivedToken;
    }

    public async Task<bool> RevokeAllUserTokens(Guid userId, CancellationToken ct)
    {
        var rowsRevoked = await _dbContext.RefreshTokens.Where(r => r.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true), ct);

        return rowsRevoked > 0;
    }

    public async Task<bool> RevokeRefreshTokenById(Guid refreshTokenId, CancellationToken ct)
    {
        var rowsUpdated = await _dbContext.RefreshTokens.Where(r => r.Id == refreshTokenId).ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true));
        return rowsUpdated > 0;
    }
}