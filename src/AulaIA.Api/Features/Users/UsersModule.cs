using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AulaIA.Api.Features.Users;

public static class UsersModule
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/me", EnsureUserAsync)
           .WithTags("Users")
           .WithName("EnsureUserProfile")
           .RequireAuthorization();

        return app;
    }

    /// <summary>
    /// Idempotente. Crea o recupera el perfil del usuario en la BD a partir de los
    /// claims del JWT de Auth0. Se debe llamar una vez por sesión (post-login).
    ///
    /// Estrategia:
    ///   1. Busca por Auth0Sub → encontrado: devuelve el perfil.
    ///   2. No encontrado → crea un nuevo User con Role = Teacher.
    /// </summary>
    private static async Task<Ok<UserProfileResponse>> EnsureUserAsync(
        HttpContext ctx,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var sub      = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? ctx.User.FindFirstValue("sub")
                    ?? throw new UnauthorizedAccessException("JWT sin claim 'sub'.");
        var email    = ctx.User.FindFirstValue("email") ?? string.Empty;
        var fullName = ctx.User.FindFirstValue("name")
                    ?? ctx.User.FindFirstValue("nickname")
                    ?? email;

        // 1. Buscar por sub exacto
        var user = await db.Users.FirstOrDefaultAsync(u => u.Auth0Sub == sub, ct);

        if (user is null)
        {
            // 2. Crear nuevo usuario Teacher
            user = new User
            {
                Auth0Sub = sub,
                Email    = email,
                FullName = fullName,
                Role     = UserRole.Teacher,
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
        }

        return TypedResults.Ok(new UserProfileResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.InstitutionId == Guid.Empty ? null : user.InstitutionId
        ));
    }
}

public sealed record UserProfileResponse(
    Guid    Id,
    string  Email,
    string  FullName,
    string  Role,
    Guid?   InstitutionId
);
