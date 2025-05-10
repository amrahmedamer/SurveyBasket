using FluentValidation;

namespace SurveyBasket.Api.Contracts.Authentication;

public class ForegetPasswordRequestValidator : AbstractValidator<ForegetPasswordRequest>
{
    public ForegetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

    }
}