namespace SurveyBasket.Api.Errors;
public static class QuestionErrors
{
    public static Error QuestionNotFound = new Error("Question.NotFound", "No Question was found with the given ID",StatusCodes.Status404NotFound);
    public static Error DuplicatedQuestionContent= new Error("Question.DuplicatedContent", "This Question has the same content.", StatusCodes.Status409Conflict);
    public static Error NoQuestion = new Error("Question.NoQuestion", "No Questions For This Poll.", StatusCodes.Status404NotFound);

}
