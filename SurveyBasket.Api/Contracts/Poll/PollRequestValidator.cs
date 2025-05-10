using FluentValidation;

namespace SurveyBasket.Api.Contracts.Poll;

public class PollRequestValidator : AbstractValidator<PollRequest>
{
    public PollRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Enter different value ");

        RuleFor(x => x.Summery)
           .NotEmpty()
           .WithMessage("Please Enter Value");

        RuleFor(x => x.StartsAt)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));

        RuleFor(x => x.EndsAt)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartsAt);

        //RuleFor(x => x)
        //    .Must(HasDateValid)
        //    .WithName(nameof(PollResponse.EndsAt))
        //    .WithMessage("{PropertyName} Must be greater than 'Start At'");

    }
    //private bool HasDateValid(PollRequest createPollRequest)
    //{
    //    return createPollRequest.EndsAt >= createPollRequest.StartsAt
    //        && createPollRequest.StartsAt >= DateOnly.FromDateTime(DateTime.Now);
    //}

}
