namespace SurveyBasket.Api.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> GetRefreshTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default);
    Task<Result> RevokedRefreshTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default);
    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result> EmailConfirmationAsync(ConfirmEmailRequest request);
    Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request);
    Task<Result> SendResetPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);




}
