using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AulaIA.Api.Shared.Domain;

public interface ICurrentUserService
{
    string Auth0Sub { get; }
    Task<User> ResolveAsync(CancellationToken ct = default);
}

public sealed class CurrentUserService(IHttpContextAccessor accessor, AulaIADbContext db) : ICurrentUserService
{
    private User? _cached;

    public string Auth0Sub =>
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? accessor.HttpContext?.User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("No authenticated user.");

    public async Task<User> ResolveAsync(CancellationToken ct = default)
    {
        if (_cached is not null) return _cached;

        var sub = Auth0Sub;
        _cached = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Auth0Sub == sub, ct)
            ?? throw new UnauthorizedAccessException($"User with sub '{sub}' not found.");

        return _cached;
    }
}
