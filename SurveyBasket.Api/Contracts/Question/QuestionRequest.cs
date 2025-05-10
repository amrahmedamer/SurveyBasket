using SurveyBasket.Api.Contracts.Answer;

namespace SurveyBasket.Api.Contracts.Question;

public record QuestionRequest(
  string Content,
    List<string> Answers
);
