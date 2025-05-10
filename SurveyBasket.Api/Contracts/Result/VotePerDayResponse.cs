namespace SurveyBasket.Api.Contracts.Result;

public record VotePerDayResponse(
    DateOnly Date,
    int NumberOfVotes
    );

