namespace SurveyBasket.Api.Errors;
public static class VoteErrors
{
    public static Error DuplicatedVote = new Error("Vote.DuplicatedVote", "This user already voted before for this poll", StatusCodes.Status409Conflict);
    public static Error InValidQuestion = new Error("Vote.InValidQuestion", "InValid Question", StatusCodes.Status400BadRequest);
    public static Error NotFound = new Error("Vote.NotFound", "No Vote was found with the given ID ", StatusCodes.Status404NotFound);

}
