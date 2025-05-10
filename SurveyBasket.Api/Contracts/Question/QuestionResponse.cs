using SurveyBasket.Api.Contracts.Answer;

namespace SurveyBasket.Api.Contracts.Question;

public record QuestionResponse(
    int Id,
    string Content,
    IEnumerable<AnswerResponse> Answers
 );
