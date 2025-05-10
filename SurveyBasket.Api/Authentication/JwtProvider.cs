
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SurveyBasket.Api.Authentication;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions _jwtOptions;

    public JwtProvider(IOptions<JwtOptions> jwtToken)
    {
        _jwtOptions = jwtToken.Value;
    }

    public (string token, int expiresIn) GetJwtToken(ApplicationUser user)
    {
        Claim[] claims = [
            new (JwtRegisteredClaimNames.Sub,user.Id),
            new (JwtRegisteredClaimNames.Email,user.Email!),
            new (JwtRegisteredClaimNames.GivenName,user.FirstName),
            new (JwtRegisteredClaimNames.FamilyName,user.LastName),
            new (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),

        ];

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecurityKey));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);


        var token = new JwtSecurityToken(
            issuer: _jwtOptions.issuer,
            audience: _jwtOptions.audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.expiresIn),
            signingCredentials: signingCredentials
        );

        return (token: new JwtSecurityTokenHandler().WriteToken(token), expiresIn: _jwtOptions.expiresIn * 60);
    }

    public string? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecurityKey));

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                IssuerSigningKey = symmetricSecurityKey,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero

            }
            , out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
           return jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
        }
        catch (Exception)
        {

            return null;
        }


    }
}
