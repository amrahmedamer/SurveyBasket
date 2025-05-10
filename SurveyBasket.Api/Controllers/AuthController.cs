using Azure;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Authentication;

namespace SurveyBasket.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(IAuthService authService, IOptions<JwtOptions> jwtOptions)
    {
        _authService = authService;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("")]
    public async Task<IActionResult> LoginAsync( [FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult.IsSuccess
           ? Ok(authResult.Value)
           : authResult.ToProblem();

    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTokenAsync( [FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        return authResult.IsSuccess
        ? Ok(authResult.Value)
        : authResult.ToProblem();

    }
    [HttpPut("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.RevokedRefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);
        return authResult.IsSuccess
         ? Ok()
         : authResult.ToProblem();

    }

    [HttpPost("register")]
    public async Task<IActionResult> register( [FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.RegisterAsync(request, cancellationToken);

        return authResult.IsSuccess
           ? Ok()
           : authResult.ToProblem();

    }
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.EmailConfirmationAsync(request);

        return authResult.IsSuccess
           ? Ok()
           : authResult.ToProblem();

    }
    [HttpPost("resend-confirm-email")]
    public async Task<IActionResult> ResendConfirmEmail([FromBody]ResendConfirmationEmailRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.ResendConfirmationEmailAsync(request);

        return authResult.IsSuccess
           ? Ok()
           : authResult.ToProblem();

    }
    [HttpPost("foreget-password")]
    public async Task<IActionResult> ForegetPassswoed([FromBody]ForegetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.SendResetPasswordAsync(request.Email);
        return Ok();
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassswoed([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result=await _authService.ResetPasswordAsync(request);
        return result.IsSuccess? Ok():NotFound(result);
    }

}
