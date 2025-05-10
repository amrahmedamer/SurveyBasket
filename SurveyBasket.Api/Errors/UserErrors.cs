
namespace SurveyBasket.Api.Errors;

public static class UserErrors
{
    public static Error UserInvalidCredential = new Error("User.InvalidCredential", "Invalid Email/Password" , StatusCodes.Status401Unauthorized);
    public static Error DuplicatedEmail = new Error("User.DuplicatedEmail", "Another user with the same email is already exists", StatusCodes.Status409Conflict);
    public static Error EmailNotConfirmed = new Error("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status401Unauthorized);
    public static Error InvalidCode = new Error("User.InvalidCode", "Invalid Code", StatusCodes.Status401Unauthorized);
    public static Error DuplicatedConfirm = new Error("User.DuplicatedConfirm", "This email is already confirmed", StatusCodes.Status409Conflict);

}
