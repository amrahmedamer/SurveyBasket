using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace SurveyBasket.Api.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtProvider jwtProvider,
    IEmailSender emailSender,
    ILogger<AuthService> logger,
    IHttpContextAccessor httpContextAccessor
    ) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly int _refreshTokenExpiryDays = 14;

    public async Task<Result<AuthResponse>> GetTokenAsync(string Email, string Password, CancellationToken cancellationToken = default)
    {
        //check user 
        if (await _userManager.FindByEmailAsync(Email) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.UserInvalidCredential);

        //check password
        var result = await _signInManager.PasswordSignInAsync(user, Password, false, false);

        if (result.Succeeded)
        {
            //generate jwt token
            var (token, expires) = _jwtProvider.GetJwtToken(user);

            //refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            //save in db
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration
            });

            await _userManager.UpdateAsync(user);

            //return authResponse
            var authResponse = new AuthResponse(user.Id, user.Email!, user.FirstName, user.LastName, token, expires, refreshToken, refreshTokenExpiration);
            return Result.Success(authResponse);
        }

        return Result.Failure<AuthResponse>(result.IsNotAllowed ? UserErrors.EmailNotConfirmed : UserErrors.UserInvalidCredential);

    }
    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(Token);
        if (userId is null)
            return Result.Failure<AuthResponse>(UserErrors.UserInvalidCredential);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure<AuthResponse>(UserErrors.UserInvalidCredential);

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == RefreshToken && x.isActive);
        if (userRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.UserInvalidCredential);

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        //generate jwt token
        var (token, expires) = _jwtProvider.GetJwtToken(user);

        //refresh token
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        //save in db
        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = newRefreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        var authResponse = new AuthResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            token,
            expires,
            newRefreshToken,
            newRefreshTokenExpiration);
        return Result.Success(authResponse);

    }
    public async Task<Result> RevokedRefreshTokenAsync(string Token, string RefreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(Token);
        if (userId is null)
            return Result.Failure(UserErrors.UserInvalidCredential);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(UserErrors.UserInvalidCredential);


        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == RefreshToken && x.isActive);
        if (userRefreshToken is null)
            return Result.Failure(UserErrors.UserInvalidCredential);


        userRefreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailIsExist = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (emailIsExist)
            return Result.Failure<AuthResponse>(UserErrors.DuplicatedEmail);

        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Code Confirmation : {code}", code);

            await SendConfirmationEmail(user, code);

            return Result.Success();
        }
        var errors = result.Errors.First();
        return Result.Failure(new Error(errors.Code, errors.Description, StatusCodes.Status400BadRequest));
    }
    public async Task<Result> EmailConfirmationAsync(ConfirmEmailRequest request)
    {
        if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCode);

        if (user.EmailConfirmed)
            return Result.Failure<AuthResponse>(UserErrors.DuplicatedConfirm);

        var code = request.Code;
        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        }
        catch (Exception)
        {

            return Result.Failure(UserErrors.InvalidCode);
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
            return Result.Success();

        var errors = result.Errors.First();
        return Result.Failure(new Error(errors.Code, errors.Description, StatusCodes.Status400BadRequest));
    }
    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirm);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Code Confirmation from Resend : {code}", code);

        await SendConfirmationEmail(user, code);

        return Result.Success();

    }
    public async Task<Result> SendResetPasswordAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed);

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Password Reset code : {code}", code);

        await SendPasswordResetEmail(user, code);

        return Result.Success();
    }
    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {

        if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Failure(UserErrors.InvalidCode);

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {

            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (result.Succeeded)
            return Result.Success();

        var errors = result.Errors.First();
         
         return Result.Failure(new Error(errors.Code, errors.Description, StatusCodes.Status401Unauthorized));
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Host;
        var bodyBuilder = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
        new Dictionary<string, string>
        {
            { "{{name}}", user.FirstName },
                { "{{action_url}}", $"{origin}/Auth/confirm-email?userId={user.Id}&code={code}" },

        });

        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Email Confirmation", bodyBuilder));
        await Task.CompletedTask;
    }
    private async Task SendPasswordResetEmail(ApplicationUser user, string code)
    {

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Host;
        var bodyBuilder = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
        new Dictionary<string, string>
        {
            { "{{name}}", user.FirstName },
            { "{{action_url}}", $"{origin}/Auth/foreget-password?email={user.Email}&code={code}" },

        });

        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Email Confirmation", bodyBuilder));
        await Task.CompletedTask;
    }

}
