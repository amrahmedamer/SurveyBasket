using FluentValidation;

namespace SurveyBasket.Api.Contracts.User;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {

        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage("Password should be at least 8 digits and should contains Uppercase,NonAlphanumeric,Lowercase and Digit")
            .NotEqual(x=>x.CurrentPassword)
            .WithMessage("New password cannot be same as the current password");
    }
}