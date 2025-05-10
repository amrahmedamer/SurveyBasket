namespace SurveyBasket.Api.Contracts.User;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
