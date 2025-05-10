namespace SurveyBasket.Api.Contracts.Result;

public record VotePerQuestionResponse(
 string Question,
 IEnumerable<VotePerAnswerResponse> VotePerAnswers 

    );

