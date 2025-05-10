namespace SurveyBasket.Api.Authentication;

public interface IJwtProvider
{
   (string token,int expiresIn) GetJwtToken (ApplicationUser user);
    string? ValidateToken(string token);
}
