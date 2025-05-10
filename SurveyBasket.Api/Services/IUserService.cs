using SurveyBasket.Api.Contracts.User;

namespace SurveyBasket.Api.Services;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetUserProfileAsync(string Id);
    Task<Result> UpdateProfileAsync(string Id, UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(string Id, ChangePasswordRequest request);
}
