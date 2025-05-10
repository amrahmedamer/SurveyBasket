namespace SurveyBasket.Api.Errors;
public static class PollErrors
{
    public static Error PollNotFound = new Error("Poll.NotFound", "No poll was found with the given ID",StatusCodes.Status404NotFound);
    public static Error DuplicatedPollTitle = new Error("Poll.DuplicatedTitle", "Another poll has the same title.", StatusCodes.Status409Conflict);

}
