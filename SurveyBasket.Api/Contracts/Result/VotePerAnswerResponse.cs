namespace SurveyBasket.Api.Contracts.Result;

public record VotePerAnswerResponse(
    string Answer,
    int Count
    );
