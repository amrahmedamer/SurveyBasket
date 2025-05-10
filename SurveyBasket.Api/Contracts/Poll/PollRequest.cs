namespace SurveyBasket.Api.Contracts.Poll;

public record PollRequest
 (
     string Title,
     string Summery,
     DateOnly StartsAt,
     DateOnly EndsAt
);
