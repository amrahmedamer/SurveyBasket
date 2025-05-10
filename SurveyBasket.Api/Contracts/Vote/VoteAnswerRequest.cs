namespace SurveyBasket.Api.Contracts.Vote;

public record VoteAnswerRequest(
    int QuestionId,
    int AnswerId
    );

