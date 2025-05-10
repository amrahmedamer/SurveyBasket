using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Api.Authentication;

public class JwtOptions
{
    [Required]
    public string SecurityKey { get; init; } = string.Empty;
    [Required]
    public string issuer { get; init; } = string.Empty;
    [Required]
    public string audience { get; init; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int expiresIn { get; init; }
}
