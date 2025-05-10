using FluentValidation;

namespace SurveyBasket.Api.Contracts.Vote;

public class VoteRequestValidator:AbstractValidator<VoteRequest>
{
    public VoteRequestValidator()
    {
        RuleFor(x => x.Answer)
            .NotEmpty();

        RuleForEach(x => x.Answer)
            .SetInheritanceValidator(v=>v.Add(new VoteAnswerRequestValidator()));
    }
}
