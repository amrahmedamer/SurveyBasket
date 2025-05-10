using FluentValidation;

namespace SurveyBasket.Api.Contracts.Authentication;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(3,100);
        
        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(3,100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage("Password should be at least 8 digits and should contains Uppercase,NonAlphanumeric,Lowercase and Digit");

       


    }
}