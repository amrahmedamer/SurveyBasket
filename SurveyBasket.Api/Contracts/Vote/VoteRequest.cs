namespace SurveyBasket.Api.Contracts.Vote;

public record VoteRequest(
    IEnumerable<VoteAnswerRequest> Answer
 );
