using Mapster;
using Microsoft.AspNetCore.Identity;
using SurveyBasket.Api.Contracts.User;

namespace SurveyBasket.Api.Services;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<UserProfileResponse>> GetUserProfileAsync(string Id)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == Id)
            .ProjectToType<UserProfileResponse>()
            .SingleOrDefaultAsync();


        return Result.Success(user!);
    }
    public async Task<Result> UpdateProfileAsync(string Id, UpdateProfileRequest request)
    {
        await _userManager.Users
            .Where (u => u.Id == Id)
            .ExecuteUpdateAsync(Setters =>
            Setters.SetProperty(x => x.FirstName, request.FirstName)
                   .SetProperty(x => x.LastName, request.LastName)
        );

        return Result.Success();
    }
    public async Task<Result> ChangePasswordAsync(string Id, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(Id);

        var result = await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}
