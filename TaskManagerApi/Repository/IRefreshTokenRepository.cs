using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public interface IRefreshTokenRepository
{

    public Task AddRefreshToken(RefreshToken token, CancellationToken ct);
    public Task<RefreshToken?> GetRefreshToken(string token, CancellationToken ct);

    public Task<bool> RevokeAllUserTokens(Guid userId, CancellationToken ct);
    public Task<bool> RevokeRefreshTokenById(Guid refreshTokenId, CancellationToken ct);
}