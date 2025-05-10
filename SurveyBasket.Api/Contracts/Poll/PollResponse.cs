namespace SurveyBasket.Api.Contracts.Poll;

public record PollResponse(
     int Id,
     string Title,
     string Summery,
     bool IsPublished,
     DateOnly StartsAt,
     DateOnly EndsAt
);
