namespace SurveyBasket.Api.Entities;

[Owned]
public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; } 
    public DateTime CreateOn { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedOn { get; set; }
    public bool isExpired => DateTime.UtcNow >= ExpiresOn;
    public bool isActive => RevokedOn is null && !isExpired;

}
