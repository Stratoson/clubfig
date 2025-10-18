namespace Clubfig.Shared.DTOs
{
    public record LoginRequest(string Email, string Password);

    public record LoginResponse(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAt,
        UserDto User
    );

    public record RefreshTokenRequest(string RefreshToken);

    public record UserDto(
        int UserId,
        string Email,
        string FirstName,
        string LastName,
        string OrganizationName,
        IEnumerable<string> Roles
    );

    public record RegisterRequest(
        string Email,
        string Passwrod,
        string FirstName,
        string LastName,
        string TenantCode
    );

    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
}
